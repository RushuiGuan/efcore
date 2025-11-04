if ($IsMacOS) {
	set-alias -n admin -v (Join $env:InstallDirectory, "Sample.Admin", "Sample.Admin");
} else {
	set-alias -n admin -v (Join $env:InstallDirectory, "Sample.Admin", "Sample.Admin.exe");
}