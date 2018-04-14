$source = "$ProjectDir$OutDir"
$target1 = "$SolutionDir$OutDir"
$target2 = "$SolutionDir\plugins\$OutDir"

$files = @(
	"$TargetName.dll", 
	"$TargetName.pdb"
	"Newtonsoft.Json.dll"
)

foreach ($f in $files) {
	cp "$source$f" "$target1"
	cp "$source$f" "$target2"
}
