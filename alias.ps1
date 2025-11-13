if ($IsMacOS) {
	set-alias -n admin -v (Join $env:InstallDirectory, "Sample.Admin", "Sample.Admin");
	set-alias -n test -v (Join $env:InstallDirectory, "Albatross.EFCore.IntegrationTest", "Albatross.EFCore.IntegrationTest");
} else {
	set-alias -n admin -v (Join $env:InstallDirectory, "Sample.Admin", "Sample.Admin.exe");
	set-alias -n test -v (Join $env:InstallDirectory, "Albatross.EFCore.IntegrationTest", "Albatross.EFCore.IntegrationTest.exe");
}