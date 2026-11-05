$ErrorActionPreference = 'Stop'
$out = Join-Path $PSScriptRoot 'learning'
Add-Type -AssemblyName System.Drawing
Add-Type -Path (Join-Path $PSScriptRoot '../../../analyzer/learning/bin/Debug/SolidWorksAIBridge.Analysis.Learning.dll')
$edgePath = Join-Path $out 'FRONT_EDGE_EVIDENCE.json'
$evidence = Get-Content $edgePath -Raw -Encoding UTF8 | ConvertFrom-Json
if ($evidence.status -ne 'CompletedBoundedRead' -or !$evidence.checkpoints[-1].documentMetadataPreserved) { throw 'Completed preserved-state native evidence required.' }
$registration = Get-Content (Join-Path $PSScriptRoot 'PROPORTIONS_REGISTRATION.json') -Raw | ConvertFrom-Json
$studioH = [double]$registration.sourceHeightMm
$studioX = [double]$registration.sourceAnchorXmm
$studioY = [double]$registration.sourceBottomMm
function Loop-Points($loop) {
    $points = [Collections.Generic.List[object]]::new()
    foreach ($segment in $loop.segmentsMm) { foreach ($point in $segment) { $points.Add($point) } }
    return ,$points
}
function Bounds($loop) {
    $points = Loop-Points $loop
    $xs = @($points | ForEach-Object { $_[0] })
    $ys = @($points | ForEach-Object { $_[1] })
    $x0 = [double]($xs | Measure-Object -Minimum).Minimum
    $y0 = [double]($ys | Measure-Object -Minimum).Minimum
    $x1 = [double]($xs | Measure-Object -Maximum).Maximum
    $y1 = [double]($ys | Measure-Object -Maximum).Maximum
    return [ordered]@{left=$x0;bottom=$y0;width=$x1-$x0;height=$y1-$y0}
}
function Native-Loop($role,$bodyName,$loopId) {
    $doc = $evidence.documents | Where-Object role -eq $role
    $body = $doc.bodies | Where-Object name -eq $bodyName
    foreach ($face in $body.faces) { foreach ($loop in $face.loops) {
        if ($loop.id -eq $loopId) {
            if (!$loop.closedLineLoop -or $loop.unsupportedCurvedEdges -ne 0) { throw 'Unsupported boundary cannot be presented as a complete line loop.' }
            return [pscustomobject]@{role=$role;body=$bodyName;face=$face.id;loop=$loop;z=$face.zMm;feature=$face.ownerFeature;reference="$role/$bodyName/$loopId";bounds=(Bounds $loop)}
        }
    } }
    throw "Missing native loop $role/$bodyName/$loopId"
}
function Rectangle-Loop($role,$bodyName,$loopId) {
    $native = Native-Loop $role $bodyName $loopId
    if ($native.loop.nativeEdgeCount -ne 4) { throw 'Rectangle requires exactly four native line edges.' }
    $points = Loop-Points $native.loop
    $xs = @($points | ForEach-Object { [math]::Round($_[0],8) } | Sort-Object -Unique)
    $ys = @($points | ForEach-Object { [math]::Round($_[1],8) } | Sort-Object -Unique)
    if ($xs.Count -ne 2 -or $ys.Count -ne 2) { throw 'Non-axis-aligned rectangle boundary.' }
    $b = $native.bounds
    $raw = [SolidWorksAIBridge.Analysis.Learning.Rect]::new($b.left,$b.bottom,$b.width,$b.height)
    $normalized = if ($role -eq 'Studio') {$raw.Normalize($studioX,$studioY,$studioH)} else {$raw.Normalize(-112.25,-49.9,180)}
    return [pscustomobject]@{native=$native;raw=$raw;normalized=$normalized}
}
$openings = @(
    [pscustomobject]@{id='O1';name='Large left host-panel opening';studio=(Rectangle-Loop Studio 'Boss-Extrude67[1]' 'f1/l0');generated=(Rectangle-Loop Generated 'Pilot001_LeftBayUpperSupport_PROVISIONAL' 'f4/l0')},
    [pscustomobject]@{id='O2';name='Right host-panel opening';studio=(Rectangle-Loop Studio 'Combine9' 'f469/l2');generated=(Rectangle-Loop Generated 'Pilot001_LargeWindowOpening_PROVISIONAL' 'f4/l1')},
    [pscustomobject]@{id='O3';name='Upper host-panel opening';studio=(Rectangle-Loop Studio 'Combine9' 'f469/l0');generated=(Rectangle-Loop Generated 'Pilot001_UpperGableWindowOpening_PROVISIONAL' 'f6/l0')}
)
$derived = foreach ($opening in $openings) {
    $s=$opening.studio.normalized; $g=$opening.generated.normalized
    [ordered]@{id=$opening.id;name=$opening.name;studioNative=$opening.studio.native.reference;generatedNative=$opening.generated.native.reference;studio=[ordered]@{widthH=$s.Width;heightH=$s.Height;centerXH=$s.Left+$s.Width/2;centerYH=$s.Bottom+$s.Height/2;aspect=$s.Aspect};generated=[ordered]@{widthH=$g.Width;heightH=$g.Height;centerXH=$g.Left+$g.Width/2;centerYH=$g.Bottom+$g.Height/2;aspect=$g.Aspect};intersectionOverUnion=$s.IntersectionOverUnion($g);registration='Provisional semantic datum/anchor alignment';geometry='Measured closed CAD host-panel loops; not complete through-body clear apertures';classification='BothPlausible';verified=$false}
}
$derived | ConvertTo-Json -Depth 10 | Set-Content (Join-Path $out 'OPENING_COMPARISON.json') -Encoding UTF8
function Surface-Measurement($role,$bodyName,$loopIds) {
    $body=($evidence.documents|Where-Object role -eq $role).bodies|Where-Object name -eq $bodyName
    $segments=[Collections.Generic.List[object]]::new();$references=[Collections.Generic.List[string]]::new()
    foreach($face in $body.faces){foreach($loop in $face.loops){if($loopIds.Count -eq 0 -or $loop.id -in $loopIds){foreach($seg in $loop.segmentsMm){$segments.Add($seg)};$references.Add("$role/$bodyName/$($loop.id)")}}}
    if($segments.Count -eq 0){return $null}
    $b=Bounds ([pscustomobject]@{segmentsMm=$segments})
    $rect=[SolidWorksAIBridge.Analysis.Learning.Rect]::new($b.left,$b.bottom,$b.width,$b.height)
    $r=if($role -eq 'Studio'){$rect.Normalize($studioX,$studioY,$studioH)}else{$rect.Normalize(-112.25,-49.9,180)}
    return [ordered]@{widthH=$r.Width;heightH=$r.Height;centerXH=$r.Left+$r.Width/2;centerYH=$r.Bottom+$r.Height/2;aspect=$r.Aspect;nativeLoopRefs=@($references);meaning='Bounding box of selected native face boundary edges; not necessarily full component or silhouette'}
}
$surfaceMeasures=[Collections.Generic.List[object]]::new()
foreach($spec in @(
    @('R1','Cut-Extrude33[1]',@(),'Pilot001_UpperGableWindowOpening_PROVISIONAL',@('f6/l1'),'Studio fascia bands versus shallow filled gable; area IoU not equivalent'),
    @('R2','Cut-Extrude31',@(),'Pilot001_LeftGableProfile_PROVISIONAL',@('f3/l0'),'Studio fascia bands versus shallow triangle; area IoU not equivalent'),
    @('M1','Combine9',@(),'Pilot001_LargeWindowOpening_PROVISIONAL',@('f4/l2'),'Source body includes upper gable; generated backing continues behind bay. Component scopes differ'),
    @('M2','Boss-Extrude67[1]',@(),'Pilot001_LeftBayUpperSupport_PROVISIONAL',@(),'Source support includes upper profile; body-level dimensions are not directly equivalent'),
    @('P1','Boss-Extrude67[1]',@('f2/l0','f70/l0'),'Pilot001_LeftBayUpperSupport_PROVISIONAL',@(),'Front selected faces only; cross-model exterior-depth datum provisional'),
    @('P2','Extrude-Thin3',@(),$null,@(),'Projection role uncertain; no generated counterpart'),
    @('E1',$null,@(),'Pilot001_LargeWindowOpening_PROVISIONAL',@('f13/l1'),'Original entrance/recess not isolated'),
    @('E2',$null,@(),'Pilot001_LargeWindowOpening_PROVISIONAL',@('f13/l0'),'Original equivalent door not isolated'),
    @('S1',$null,@(),'Pilot001_OpeningSurround_PROVISIONAL',@('f0/l0'),'Source surround not isolated; outer boundary only')
)){
    $s=if($spec[1]){Surface-Measurement Studio $spec[1] $spec[2]}else{$null}
    $g=if($spec[3]){Surface-Measurement Generated $spec[3] $spec[4]}else{$null}
    $surfaceMeasures.Add([ordered]@{id=$spec[0];studio=$s;generated=$g;limitation=$spec[5];intersectionOverUnion=$null;depthDifference=$null;verified=$false})
}
$surfaceMeasures|ConvertTo-Json -Depth 12|Set-Content (Join-Path $out 'SURFACE_MEASUREMENTS.json') -Encoding UTF8
$generatedBase=(Native-Loop Generated 'Pilot001_LargeWindowOpening_PROVISIONAL' 'f4/l2').z
$planeMeasures=@(
    [ordered]@{id='E1-recess';role='Generated';valueMm=$generatedBase-(Native-Loop Generated 'Pilot001_LargeWindowOpening_PROVISIONAL' 'f13/l1').z;kind='Within-model entrance recess plane gap';crossModelMatchVerified=$false},
    [ordered]@{id='P1-bay';role='Generated';valueMm=(Native-Loop Generated 'Pilot001_LeftBayUpperSupport_PROVISIONAL' 'f4/l1').z-$generatedBase;kind='Within-model bay projection plane gap';crossModelMatchVerified=$false},
    [ordered]@{id='S1-projection';role='Generated';valueMm=(Native-Loop Generated 'Pilot001_OpeningSurround_PROVISIONAL' 'f0/l0').z-$generatedBase;kind='Within-model ring projection plane gap';crossModelMatchVerified=$false},
    [ordered]@{id='P1-bay-candidate';role='Studio';valueMm=(Native-Loop Studio 'Boss-Extrude67[1]' 'f2/l0').z-(Native-Loop Studio 'Combine9' 'f10/l1').z;kind='Named source plane gap; architectural datum semantics unverified';crossModelMatchVerified=$false},
    [ordered]@{id='P2-plane-gap';role='Studio';valueMm=(Native-Loop Studio 'Extrude-Thin3' 'f8/l0').z-(Native-Loop Studio 'Extrude-Thin3' 'f9/l0').z;kind='Named source plane gap; projection role unverified';crossModelMatchVerified=$false}
)
$planeMeasures|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'PLANE_MEASUREMENTS.json') -Encoding UTF8

# Draw the same primitives to SVG and PNG; no point-cloud hulls, synthetic roof lines,
# curved-edge chords, hidden-line or full-building-silhouette claims.
function Start-Canvas($title,$subtitle) {
    $script:svg=[Text.StringBuilder]::new()
    [void]$script:svg.AppendLine('<svg xmlns="http://www.w3.org/2000/svg" width="1100" height="900" viewBox="0 0 1100 900"><rect width="1100" height="900" fill="#ffffff"/>')
    $script:bitmap=[Drawing.Bitmap]::new(1100,900)
    $script:graphics=[Drawing.Graphics]::FromImage($script:bitmap)
    $script:graphics.Clear([Drawing.Color]::White)
    $script:graphics.SmoothingMode=[Drawing.Drawing2D.SmoothingMode]::AntiAlias
    Text 35 40 $title '#162333' 24
    Text 35 72 $subtitle '#445568' 14
    Text 35 100 'Same XY frame and uniform height normalization; source facade datums remain provisional.' '#445568' 14
    foreach($fraction in @(0,.25,.5,.75,1)) {
        Line 130 (740-590*$fraction) 990 (740-590*$fraction) '#e3e8ee' 1
        Text 70 (745-590*$fraction) ($fraction.ToString('0.00',[cultureinfo]::InvariantCulture)) '#667788' 12
    }
    Text 35 130 'Y / H' '#667788' 12
    Text 885 778 'X / H  ->' '#667788' 12
}
function Line($x1,$y1,$x2,$y2,$color,$width=1.2) {
    $ci=[cultureinfo]::InvariantCulture
    [void]$script:svg.AppendLine(('<line x1="{0}" y1="{1}" x2="{2}" y2="{3}" stroke="{4}" stroke-width="{5}"/>' -f $x1.ToString('F3',$ci),$y1.ToString('F3',$ci),$x2.ToString('F3',$ci),$y2.ToString('F3',$ci),$color,$width.ToString($ci)))
    $pen=[Drawing.Pen]::new([Drawing.ColorTranslator]::FromHtml($color),[single]$width)
    try{$script:graphics.DrawLine($pen,[single]$x1,[single]$y1,[single]$x2,[single]$y2)}finally{$pen.Dispose()}
}
function Text($x,$y,$text,$color='#162333',$size=14) {
    $escaped=[Security.SecurityElement]::Escape($text)
    [void]$script:svg.AppendLine("<text x='$x' y='$y' font-family='Arial,sans-serif' font-size='$size' fill='$color'>$escaped</text>")
    $font=[Drawing.Font]::new('Arial',[single]$size,[Drawing.FontStyle]::Regular,[Drawing.GraphicsUnit]::Pixel)
    $brush=[Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml($color))
    try{$script:graphics.DrawString($text,$font,$brush,[single]$x,[single]($y-$size))}finally{$font.Dispose();$brush.Dispose()}
}
function Rect-Fill($r,$color) {
    $x=170+590*$r.Left;$y=740-590*($r.Bottom+$r.Height);$w=590*$r.Width;$h=590*$r.Height
    $ci=[cultureinfo]::InvariantCulture
    [void]$script:svg.AppendLine(('<rect x="{0}" y="{1}" width="{2}" height="{3}" fill="{4}"/>' -f $x.ToString('F3',$ci),$y.ToString('F3',$ci),$w.ToString('F3',$ci),$h.ToString('F3',$ci),$color))
    $brush=[Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml($color))
    try{$script:graphics.FillRectangle($brush,[single]$x,[single]$y,[single]$w,[single]$h)}finally{$brush.Dispose()}
}
function Rect-Outline($r,$color,$width=1.8) {
    $x=170+590*$r.Left;$y=740-590*$r.Bottom;$w=590*$r.Width;$h=590*$r.Height
    Line $x $y ($x+$w) $y $color $width
    Line ($x+$w) $y ($x+$w) ($y-$h) $color $width
    Line ($x+$w) ($y-$h) $x ($y-$h) $color $width
    Line $x ($y-$h) $x $y $color $width
}
function Finish-Canvas($name) {
    [void]$script:svg.AppendLine('</svg>')
    $script:svg.ToString() | Set-Content (Join-Path $out ($name+'.svg')) -Encoding UTF8
    $script:bitmap.Save((Join-Path $out ($name+'.png')),[Drawing.Imaging.ImageFormat]::Png)
    $script:graphics.Dispose();$script:bitmap.Dispose()
}
foreach($role in @('Studio','Generated')) {
    $title=if($role -eq 'Studio'){'Original Studio CAD'}else{'Generated AUE R2 CAD'}
    $color=if($role -eq 'Studio'){'#2463a0'}else{'#c66a13'}
    Start-Canvas $title 'FRONT elevation of selected native planar-face boundaries (partial; not hidden-line silhouette)'
    $doc=$evidence.documents|Where-Object role -eq $role
    foreach($body in $doc.bodies){foreach($face in $body.faces){foreach($loop in $face.loops){foreach($seg in $loop.segmentsMm){
        if($role -eq 'Studio'){$x1=($seg[0][0]-$studioX)/$studioH;$y1=($seg[0][1]-$studioY)/$studioH;$x2=($seg[1][0]-$studioX)/$studioH;$y2=($seg[1][1]-$studioY)/$studioH}
        else{$x1=($seg[0][0]+112.25)/180;$y1=($seg[0][1]+49.9)/180;$x2=($seg[1][0]+112.25)/180;$y2=($seg[1][1]+49.9)/180}
        Line (170+590*$x1) (740-590*$y1) (170+590*$x2) (740-590*$y2) $color 1.5
    }}}}
    # Matching IDs are annotations. ? means architectural correspondence unverified.
    foreach($tag in @(@('R1?',.69,1.04),@('R2?',.16,.80),@('M1?',1.20,.49),@('M2 / P1?',.13,.12))) {Text (170+590*$tag[1]) (740-590*$tag[2]) $tag[0] '#303b48' 17}
    foreach($opening in $openings){$r=if($role -eq 'Studio'){$opening.studio.normalized}else{$opening.generated.normalized}; Text (170+590*($r.Left+$r.Width/2)-12) (740-590*($r.Bottom+$r.Height/2)) ($opening.id+'?') '#a02a46' 17}
    if($role -eq 'Generated') {
        Text (170+590*.67) (740-590*.06) 'E1 / E2' '#a02a46' 14
        Text (170+590*1.05) (740-590*.17) 'S1?' '#a02a46' 14
    } else {
        Text (170+590*1.20) (740-590*.39) 'P2?' '#a02a46' 14
        Text 35 808 'E1/E2, S1: no verified source counterpart isolated. P2?: native nearby projection, role uncertain.' '#8c3b2d' 14
    }
    Text 35 835 'R1/R2 roofs; O1/O2/O3 host-panel openings; M1/M2 masses; P1 bay projection.' '#445568' 14
    Text 35 862 'E1 entrance recess; E2 door; S1 surround. ? = provisional correspondence; no geometry invented.' '#445568' 14
    Finish-Canvas ($role.ToLowerInvariant()+'-front')
}
Start-Canvas 'Supported difference overlay: O1 / O2 / O3' 'Measured closed rectangular host-panel loops; overlap is conditional on provisional registration.'
foreach($opening in $openings){
    $s=$opening.studio.normalized;$g=$opening.generated.normalized
    Rect-Fill $s '#dceafa'; Rect-Fill $g '#fae2c8'
    $ix=[math]::Max($s.Left,$g.Left);$iy=[math]::Max($s.Bottom,$g.Bottom)
    $iw=[math]::Min($s.Left+$s.Width,$g.Left+$g.Width)-$ix;$ih=[math]::Min($s.Bottom+$s.Height,$g.Bottom+$g.Height)-$iy
    if($iw -gt 0 -and $ih -gt 0){Rect-Fill ([SolidWorksAIBridge.Analysis.Learning.Rect]::new($ix,$iy,$iw,$ih)) '#d6c8e7'}
    Rect-Outline $s '#2463a0';Rect-Outline $g '#c66a13'
    Text (170+590*[math]::Min($s.Left,$g.Left)) (728-590*[math]::Max($s.Bottom+$s.Height,$g.Bottom+$g.Height)) ($opening.id+'?') '#303b48' 16
}
Text 35 810 'Blue: Studio    Orange: AUE R2    Purple: intersection. Neither color indicates correctness.' '#445568' 16
Text 35 842 'Roof/mass silhouette areas and depth are excluded: incomplete equivalent-region evidence.' '#8c3b2d' 14
Text 35 870 'Native face loops support these rectangles; they do not prove unobstructed through-body apertures.' '#445568' 14
Finish-Canvas 'opening-differences'

$assets=@('FRONT_EDGE_EVIDENCE.json','../PROPORTIONS_FRONT.json','../PROPORTIONS_REGISTRATION.json','../../references/annotated_building_boundary.png.png')
$manifest=foreach($relative in $assets){$file=Get-Item (Join-Path $out $relative);[ordered]@{path=$relative;bytes=$file.Length;sha256=(Get-FileHash $file.FullName).Hash}}
$manifest|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'INPUT_MANIFEST.json') -Encoding UTF8
$records=[ordered]@{Version='0.1.0';Scope='FrontFacade';ReferenceAuthority='CompetingEvidence';TargetHeightMm=180;Evidence=[Collections.Generic.List[object]]::new();Correspondences=[Collections.Generic.List[object]]::new();Findings=[Collections.Generic.List[object]]::new();Recipes=[Collections.Generic.List[object]]::new();Proposals=[Collections.Generic.List[object]]::new();Experiments=[Collections.Generic.List[object]]::new()}
$records.Evidence.Add([ordered]@{Id='native-edges';Asset='FRONT_EDGE_EVIDENCE.json';Entity='selected-front-faces';Method='BoundedNativeLineLoops';Status='Measured';Confidence=1;Limitation='Selected snapshot only; see exact face/loop IDs and input hash'})
$records.Evidence.Add([ordered]@{Id='registration';Asset='../PROPORTIONS_REGISTRATION.json';Entity='facade-datums';Method='CandidateRoleRegistration';Status='Inferred';Confidence=.6;Limitation='Scope/anchor not visually approved; same XY, normalized height'})
$records.Evidence.Add([ordered]@{Id='photo';Asset='../../references/annotated_building_boundary.png.png';Entity='front-region';Method='PhotoReview';Status='Inferred';Confidence=.5;Limitation='Oblique, occluded; no calibrated camera/metric rectification; additional original front photos unavailable'})
$records.Evidence.Add([ordered]@{Id='depth';Asset='FRONT_EDGE_EVIDENCE.json';Entity='cross-model-facade-depth';Method='PlaneRegistration';Status='Unknown';Confidence=0;Limitation='Parallel face normals measured, exterior-plane semantic correspondence remains provisional; no certified depth deviations'})
$maps=@(
    @('R1','Main roof',@('Cut-Extrude33[1]','Split4[1]','Split4[2]'),@('Pilot001_MainGableProfile_PROVISIONAL'),'IntentionalSimplification','Bands/sloped roof versus filled shallow gable; outline faithfulness unresolved'),
    @('R2','Left roof',@('Cut-Extrude31','Split4[3]','Split4[4]'),@('Pilot001_LeftGableProfile_PROVISIONAL'),'IntentionalSimplification','Source roof construction differs from explicitly simplified R2 silhouette'),
    @('O1','Large left host-panel opening',@('Boss-Extrude67[1]/f1/l0'),@('Pilot001_LeftBayUpperSupport_PROVISIONAL/f4/l0','Pilot001_LargeWindowOpening_PROVISIONAL/f4/l0'),'BothPlausible','Native host-panel boundary differs; oblique photo cannot adjudicate dimensions'),
    @('O2','Right host-panel opening',@('Combine9/f469/l2'),@('Pilot001_LargeWindowOpening_PROVISIONAL/f4/l1'),'BothPlausible','Square source backing opening versus taller R2; no calibrated photo adjudication'),
    @('O3','Upper host-panel opening',@('Combine9/f469/l0'),@('Pilot001_UpperGableWindowOpening_PROVISIONAL/f6/l0'),'InsufficientEvidence','Presence user-supported; exact photo proportions inaccessible/occluded'),
    @('M1','Main facade mass',@('Combine9','Extrude-Thin1'),@('MainWall','WindowCut','Pilot001_EntranceRecess_PROVISIONAL','Pilot001_LargeWindowOpening_PROVISIONAL'),'IntentionalSimplification','Thin source panels versus 12 mm pilot backing: different manufacturing/blockout intents'),
    @('M2','Left bay support',@('Boss-Extrude67[1]','Boss-Extrude1','Extrude-Thin4'),@('Pilot001_LeftBayBase_PROVISIONAL','Pilot001_LeftBayUpperSupport_PROVISIONAL'),'BothPlausible','Mass roles provisional; base obscured in photo'),
    @('P1','Bay projection',@('Boss-Extrude67[1]'),@('Pilot001_LeftBayUpperSupport_PROVISIONAL'),'InsufficientEvidence','Cross-model exterior datum/depth semantics not approved'),
    @('P2','Right-opening related projection',@('Extrude-Thin3'),@(),'InsufficientEvidence','Native projection measured; exact architectural role and AUE counterpart unknown'),
    @('E1','Entrance recess',@(),@('Pilot001_EntranceRecess_PROVISIONAL'),'InsufficientEvidence','Original recess face/component not isolated; vegetation occlusion'),
    @('E2','Door opening',@(),@('Pilot001_DoorOpening_PROVISIONAL'),'InsufficientEvidence','Source notch not classified as equivalent door; no rectangle fabricated'),
    @('S1','OpeningSurround',@(),@('Pilot001_OpeningSurround_PROVISIONAL'),'InsufficientEvidence','Source local surround not isolated; original need not contain equivalent separate body')
)
foreach($m in $maps){
    $records.Correspondences.Add([ordered]@{Id=$m[0];Component=$m[1];StudioEntities=@($m[2]);GeneratedEntities=@($m[3]);EvidenceIds=@('native-edges','registration','photo');Confidence= $(if($m[2].Count -gt 0 -and $m[3].Count -gt 0){.6}else{.2});Verified=$false;Limitation=$m[5]})
    $records.Findings.Add([ordered]@{Id=('finding-'+$m[0]);CorrespondenceId=$m[0];Classification=$m[4];Verified=$false;EvidenceIds=@('native-edges','registration','photo');Rationale=$m[5]})
}
foreach($r in $derived){foreach($role in @('studio','generated')){foreach($quantity in @('widthH','heightH','centerXH','centerYH','aspect')){
    $records.Evidence.Add([ordered]@{Id=($r.id+'-'+$role+'-'+$quantity);Asset='OPENING_COMPARISON.json';Entity=($r.id+'/'+$role);Method='NativeLoopMeasurementWithProvisionalRegistration';Status='Measured';Value=$r[$role][$quantity];Units='dimensionless';Confidence=.6;Limitation='Measured coordinates; semantic registration provisional'})
}};$records.Evidence.Add([ordered]@{Id=($r.id+'-IoU');Asset='OPENING_COMPARISON.json';Entity=$r.id;Method='RegisteredHostPanelRectangleIoU';Status='Measured';Value=$r.intersectionOverUnion;Units='dimensionless';Confidence=.6;Limitation='Conditional on role and datum correspondence; no true silhouette IoU'})}
foreach($r in $surfaceMeasures){foreach($role in @('studio','generated')){foreach($quantity in @('widthH','heightH','centerXH','centerYH','aspect')){
    if($r[$role]){$records.Evidence.Add([ordered]@{Id=($r.id+'-'+$role+'-'+$quantity);Asset='SURFACE_MEASUREMENTS.json';Entity=($r.id+'/'+$role);Method='SelectedNativeFaceBoundaryBoundingBox';Status='Measured';Value=$r[$role][$quantity];Units='dimensionless';Confidence=.5;Limitation=$r.limitation})}
    else{$records.Evidence.Add([ordered]@{Id=($r.id+'-'+$role+'-'+$quantity);Asset='SURFACE_MEASUREMENTS.json';Entity=($r.id+'/'+$role);Method='ComponentCorrespondence';Status='Unknown';Confidence=0;Limitation=$r.limitation})}
}}}
$records.Evidence.Add([ordered]@{Id='prior-r2-baseline-test';Asset='../../results/R2_RESTART_ATTEMPT_2026-11-05/RESULT.md';Entity='historical-r2-baseline';Method='HistoricalBaselineCadTest';Status='Passed';Confidence=1;Limitation='Historical successful single R2 build/rebuild, not a test of any new correction or recipe'})
foreach($p in $planeMeasures){$records.Evidence.Add([ordered]@{Id=($p.role+'-'+$p.id);Asset='PLANE_MEASUREMENTS.json';Entity=$p.id;Method='SelectedParallelCadPlaneGap';Status='Measured';Value=$p.valueMm;Units='mm';Confidence=1;Limitation=$p.kind+'; not a certified cross-model architectural depth difference'})}
foreach($recipe in @(@('recipe-studio','Studio thin-panel / roof-band construction',@('Extrude-Thin1','Combine9','Boss-Extrude17','Boss-Extrude9'),@('BossThin','CombineBodies','Boss','Split','Cut'),'Body group/merge scope, dimensions and edits not validated; full dependency closure not read'),@('recipe-r2','R2 constrained blockout profiles',@('MainWall','Pilot001_LeftBayBase_PROVISIONAL','Pilot001_LeftBayUpperSupport_PROVISIONAL','Pilot001_OpeningSurround_PROVISIONAL'),@('Extrusion','Boss','Cut'),'Baseline rebuilds passed previously; independent edits, manufacturing profile and source-role match unvalidated'))){
    $role=if($recipe[0] -eq 'recipe-studio'){'Studio'}else{'Generated'}
    $doc=$evidence.documents|Where-Object role -eq $role
    $parents=@(foreach($entity in $recipe[2]){$property=$doc.recipes.PSObject.Properties[$entity];if($property){$property.Value.directParents}})|Sort-Object -Unique
    $records.Recipes.Add([ordered]@{Id=$recipe[0];Component=$recipe[1];Entities=$recipe[2];FeatureTypes=$recipe[3];Dependencies=@($parents);EvidenceIds=@('native-edges');Status='Draft';Risks=$recipe[4]})
}
foreach($doc in $evidence.documents){foreach($property in $doc.recipes.PSObject.Properties){$feature=$property.Value;if(!$feature.missing){
    $records.Recipes.Add([ordered]@{Id=('feature-recipe-'+$doc.role+'-'+$feature.name);Component='Observed feature construction; architectural assignment provisional';Entities=@($doc.role+'/'+$feature.name);FeatureTypes=@($feature.resolvedType);Dependencies=@($feature.directParents);EvidenceIds=@('native-edges');Status='Draft';Risks='Direct parents only; definition, exact feature body scope, historical construction intent and edit reliability not established'})
}}}
$records.Recipes.Add([ordered]@{Id='recipe-ring-nested';Component='S1 equivalent isolated surround fixture';Entities=@('Generated/Pilot001_OpeningSurround_PROVISIONAL');FeatureTypes=@('Boss');Dependencies=@('Generated/Pilot001_OpeningSurroundSketch');EvidenceIds=@('native-edges');Status='Draft';Risks='Observed R2 nested-profile ring baseline; independent border edits and manufacturing qualification not established'})
$records.Recipes.Add([ordered]@{Id='recipe-ring-four-profiles';Component='S1 equivalent isolated surround fixture';Entities=@();FeatureTypes=@('PlannedBoss');Dependencies=@('Planned shared width/height/border parameters');EvidenceIds=@('native-edges');Status='Draft';Risks='Unimplemented alternative: four non-overlapping border profiles sharing corner edges; union/corner connectivity and independent edits must be tested in isolation, not assumed'})
$records.Proposals.Add([ordered]@{Id='proposal-opening-study';FindingId='finding-S1';RecipeIds=@('recipe-ring-nested','recipe-ring-four-profiles');Action='First verify role/plane/photo evidence; then separately authorize equivalent-geometry nested-ring versus four-border-profile fixture, preserving R2 and all studio originals. This tests construction, not which architectural interpretation is correct';Status='Draft'})
$records.Experiments.Add([ordered]@{Id='experiment-opening-study';ProposalId='proposal-opening-study';FixtureId='planned-front-opening-study-v1';Stage='Measure';Outcome='Pending';EvidenceIds=@('native-edges');RegressionEvidenceIds=@();HumanApproved=$false})
$json=$records|ConvertTo-Json -Depth 20
$typed=[SolidWorksAIBridge.Analysis.Learning.LearningRecord]::FromJson($json)
$typed.ToJson()|Set-Content (Join-Path $out 'PILOT_LEARNING_RECORD.json') -Encoding UTF8
$html=@'
<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Pilot 001 — Evidence comparison</title><style>body{font:16px system-ui;margin:32px;background:#f3f5f8;color:#162333}h1{font-size:28px}.note{max-width:1000px;line-height:1.6}.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(450px,1fr));gap:20px}figure{margin:0;background:white;padding:16px;border:1px solid #d5dce5;border-radius:10px}img{width:100%;height:auto}a{color:#19568b}summary{cursor:pointer;padding:12px}code{background:#e5eaf0}li{margin:10px 0}</style><h1>Pilot #001 — FRONT evidence comparison</h1><p class="note">Neither model is ground truth. Same orientation, normalized architectural height and provisional XY frame. These are selected native face boundaries, not vertex-cloud silhouettes or complete hidden-line elevations. Every <b>?</b> marks an unverified architectural correspondence. R2 stays unchanged at 180 mm.</p><div class="grid"><figure><a href="studio-front.svg"><img src="studio-front.png" alt="Original Studio CAD selected front planar-face boundaries with provisional IDs"></a><figcaption>Original Studio CAD — currently open dirty state, not proven saved state.</figcaption></figure><figure><a href="generated-front.svg"><img src="generated-front.png" alt="Generated AUE R2 selected front planar-face boundaries with matching IDs"></a><figcaption>Generated AUE R2 CAD — saved, unchanged experimental baseline.</figcaption></figure></div><figure style="max-width:1000px;margin-top:20px"><a href="opening-differences.svg"><img src="opening-differences.png" alt="Conditional registered difference overlay for three host-panel rectangular openings"></a><figcaption>Supported rectangle overlap only. Roof, mass silhouette, entry and surround comparisons remain partial or unavailable.</figcaption></figure><h2>Evidence and next action</h2><ul><li><a href="COMPONENT_REPORT.md">Component and confidence report</a></li><li><a href="RECIPE_REPORT.md">First feature-recipe report</a></li><li><a href="CONTROLLED_EXPERIMENT.md">Draft controlled experiment</a></li><li><a href="PILOT_LEARNING_RECORD.json">Validated provisional learning record</a></li><li><a href="FRONT_EDGE_EVIDENCE.json">Native face-loop and feature-parent evidence</a></li><li><a href="OPENING_COMPARISON.json">Normalized measurements and conditional IoU</a></li><li><a href="TEST_RESULTS.md">Executed automated checks and limitations</a></li></ul></html>
'@
$html|Set-Content (Join-Path $out 'index.html') -Encoding UTF8
Write-Output 'VISUALS_AND_LEARNING_RECORD_CREATED'
foreach($r in $derived){Write-Output ($r.id+' conditional IoU='+$r.intersectionOverUnion)}
