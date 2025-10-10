$path="Ozge.App\Presentation\Windows\TeacherDashboardWindow.xaml"
$lines=[System.Collections.Generic.List[string]]::new();$lines.AddRange([string[]](Get-Content $path));
function RemoveBlock([string]$marker){$start=-1;$depth=0;for($i=0;$i -lt $lines.Count;$i++){if($lines[$i].Contains($marker)){$start=$i;break;}}if($start -lt 0){throw "Marker $marker not found";}for($i=$start;$i -lt $lines.Count;$i++){if($lines[$i].Contains("<Grid")){$depth++}if($lines[$i].Contains("</Grid>")){$depth--}$lines.RemoveAt($i);$i--;if($depth -le 0){break;}}return $start;}
