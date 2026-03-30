if ($IsMacOS) {
	set-alias -n admin -v (Join $env:InstallDirectory, "Crm.Admin", "Crm.Admin");
} else {
	set-alias -n admin -v (Join $env:InstallDirectory, "Crm.Admin", "Crm.Admin.exe");
}