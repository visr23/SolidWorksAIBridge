$ErrorActionPreference='Stop'
$app=[Runtime.InteropServices.Marshal]::GetActiveObject('SldWorks.Application')
$doc=$app.ActiveDoc
if($null -eq $doc -or $doc.GetTitle() -ne 'Part1' -or $doc.GetPathName() -ne ''){throw 'Unexpected document; refusing inspection'}
Write-Output 'READ_ONLY_INSPECTION Part1; unsaved; no rebuild/mutation/save calls'
foreach($name in @('Width@WallSketch','Height@WallSketch','D1@MainWall','WindowWidth@WindowSketch','WindowHeight@WindowSketch','CenterX@WindowSketch','CenterY@WindowSketch','D1@WindowCut')){
 $dimension=$doc.Parameter($name)
 if($null -eq $dimension){throw "Missing $name"}
 Write-Output ('{0} = {1:R} mm; DrivenState={2}' -f $name,($dimension.SystemValue*1000),$dimension.DrivenState)
}
