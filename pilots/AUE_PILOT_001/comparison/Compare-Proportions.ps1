$ErrorActionPreference = 'Stop'
$cad = Get-Content (Join-Path $PSScriptRoot 'PROPORTIONS_FRONT.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$pilot = $cad.documents | Where-Object title -eq 'AUE_PILOT_001_FRONT_V1'
$studio = $cad.documents | Where-Object title -eq 'Part1'
if (!$pilot -or !$studio) { throw 'Both selected documents are required.' }
$main = $studio.bodies | Where-Object name -eq 'Combine9'
$bay = $studio.bodies | Where-Object name -eq 'Boss-Extrude67[1]'
$roof = $studio.bodies | Where-Object name -eq 'Cut-Extrude33[1]'
$leftRoof = $studio.bodies | Where-Object name -eq 'Cut-Extrude31'
foreach ($body in @($main,$bay,$roof,$leftRoof)) {
    if ($body.verticesMm.Count -ne $body.vertexCount) { throw "Incomplete selected vertex set: $($body.name)" }
}
$bottom = $bay.extremePointsMm[3][1]
$height = $roof.extremePointsMm[2][1] - $bottom
$anchor = $bay.extremePointsMm[1][0]
$back = $main.extremePointsMm[5][2]
$rightPoints = @($main.verticesMm | Where-Object { [math]::Abs($_[2]-$back) -lt 0.00001 -and $_[0] -gt 60 -and $_[0] -lt 100 -and $_[1] -lt 30 })
$upperPoints = @($main.verticesMm | Where-Object { [math]::Abs($_[2]-$back) -lt 0.00001 -and $_[0] -gt 0 -and $_[1] -gt 60 -and $_[1] -lt 110 })
$largePoints = @($bay.verticesMm | Where-Object { [math]::Abs($_[2]) -lt 0.00001 -and $_[0] -gt -120 -and $_[0] -lt 0 -and $_[1] -ge -37.00001 -and $_[1] -le 28.00001 })
function Rectangle-Measure($points) {
    if ($points.Count -ne 4) { throw "Expected four aperture candidate corner points, got $($points.Count)" }
    $xs = @($points | ForEach-Object { [math]::Round($_[0],9) } | Sort-Object -Unique)
    $ys = @($points | ForEach-Object { [math]::Round($_[1],9) } | Sort-Object -Unique)
    if ($xs.Count -ne 2 -or $ys.Count -ne 2) { throw 'Candidate corners are not rectangular.' }
    return [ordered]@{ width=$xs[1]-$xs[0];height=$ys[1]-$ys[0];centerX=($xs[0]+$xs[1])/2;centerY=($ys[0]+$ys[1])/2;points=$points }
}
$right = Rectangle-Measure $rightPoints
$upper = Rectangle-Measure $upperPoints
$large = Rectangle-Measure $largePoints
$rows = [Collections.Generic.List[object]]::new()
function Metric($label,$pilotValue,$sourceValue,$kind='length') {
    $p = $pilotValue/180
    $s = $sourceValue/$height
    $rows.Add([pscustomobject]@{metric=$label;pilotOverHeight=$p;sourceOverHeight=$s;deltaInHeightPercentagePoints=100*($p-$s);relativeDifferencePercent=100*($p/$s-1);sourceEquivalentAt180mm=180*$s;pilotMm=$pilotValue;sourceMm=$sourceValue;classification='Measured coordinates; provisional facade/semantic correspondence';kind=$kind})
}
Metric 'Roof envelope width' 243 ($roof.extremePointsMm[0][0]-$leftRoof.extremePointsMm[1][0])
Metric 'Lower ridge height above selected bottom' 131.1 ($leftRoof.extremePointsMm[2][1]-$bottom)
foreach ($item in @(@('Right aperture',$right,23.4,30.4,167.1,46.8),@('Large left aperture',$large,82.9,48.7,49.45,52.05),@('Upper aperture',$upper,48.8,35.1,133.0,121.85))) {
    Metric ($item[0]+' width') $item[2] $item[1].width
    Metric ($item[0]+' height') $item[3] $item[1].height
    Metric ($item[0]+' center X') $item[4] ($item[1].centerX-$anchor) 'position'
    Metric ($item[0]+' center Y') $item[5] ($item[1].centerY-$bottom) 'position'
}
$rows | Export-Csv (Join-Path $PSScriptRoot 'PROPORTIONS_METRICS.csv') -NoTypeInformation -Encoding UTF8
[ordered]@{sourceBottomMm=$bottom;sourceHeightMm=$height;sourceAnchorXmm=$anchor;sourceCandidates=@('Combine9','Boss-Extrude67[1]','Cut-Extrude33[1]','Cut-Extrude31');sourceRight=$right;sourceUpper=$upper;sourceLarge=$large;status='PROVISIONAL registration; not certified front silhouette or through-aperture metrology';normalization='XY/H, no CAD scale changes; depth registration pending'} | ConvertTo-Json -Depth 10 | Set-Content (Join-Path $PSScriptRoot 'PROPORTIONS_REGISTRATION.json') -Encoding UTF8
$svg = [Text.StringBuilder]::new()
[void]$svg.AppendLine('<svg xmlns="http://www.w3.org/2000/svg" width="1000" height="760" viewBox="0 0 1000 760"><rect width="1000" height="760" fill="#fff"/><text x="30" y="30" font-family="sans-serif" font-size="20">Provisional front XY registration: height = 1</text><text x="30" y="57" font-family="sans-serif" font-size="14">Blue: studio selected vertices. Orange: R2 vertices. Points are not reconstructed outlines.</text>')
foreach ($pair in @(@($pilot,'#e36a17'),@($studio,'#1267b0'))) {
    foreach ($body in $pair[0].bodies) {
        if ($pair[0].title -eq 'Part1' -and $body.name -notin @('Combine9','Boss-Extrude67[1]','Cut-Extrude33[1]','Cut-Extrude31')) { continue }
        foreach ($point in $body.verticesMm) {
            if ($pair[0].title -eq 'Part1') { $x=($point[0]-$anchor)/$height; $y=($point[1]-$bottom)/$height }
            else { $x=($point[0]+112.25)/180; $y=($point[1]+49.9)/180 }
            $px = (120+570*$x).ToString('F3',[cultureinfo]::InvariantCulture)
            $py = (690-570*$y).ToString('F3',[cultureinfo]::InvariantCulture)
            [void]$svg.AppendLine("<circle cx='$px' cy='$py' r='1.7' fill='$($pair[1])' fill-opacity='.6'/>")
        }
    }
}
[void]$svg.AppendLine('<text x="30" y="738" font-family="sans-serif" font-size="14">Source height/anchor inferred from selected facade bodies. No depth or occlusion registration.</text></svg>')
$svg.ToString() | Set-Content (Join-Path $PSScriptRoot 'PROPORTIONS_OVERLAY.svg') -Encoding UTF8
$rows | Select-Object metric,@{n='Pilot/H';e={[math]::Round($_.pilotOverHeight,4)}},@{n='Source/H';e={[math]::Round($_.sourceOverHeight,4)}},@{n='RelativeDeltaPercent';e={[math]::Round($_.relativeDifferencePercent,2)}} | Format-Table -AutoSize
Write-Output "SOURCE_CANDIDATE_HEIGHT=$height; SOURCE_ANCHOR_X=$anchor; SOURCE_BOTTOM=$bottom"
