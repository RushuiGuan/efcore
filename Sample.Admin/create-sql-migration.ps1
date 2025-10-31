$location = Get-Location;
Set-Location $PSScriptRoot;

if(test-Path $PSScriptRoot\Migrations){
	$count = (Get-Childitem $PSScriptRoot\Migrations\ -recurse -filter *_SampleSqlServerMigration_v*.Designer.cs  | Measure-Object).Count + 1;
}else{
	$count = 1;
}

$name = "v$count";
"Creating migration $name";
dotnet ef migrations add SampleSqlServerMigration_$name --context SampleSqlServerMigration --output-dir (Join Migrations, SqlServer)

"PreMigrationScripts";
dotnet run -- sqlserver exec-script $PSScriptRoot\PreMigrationScripts;

"Migrate";
dotnet run --no-build -- sqlserver ef-migrate

"PostMigrationScripts";
dotnet run --no-build -- sqlserver exec-script $PSScriptRoot\Scripts

"Generate scripts"
dotnet run --no-build -- sqlserver create-script --output-file (Join $PSScriptRoot, "db", "sqlserver", "tables.sql") --drop-script (Join $PSScriptRoot, "db", "sqlserver", "drop.sql")

Set-Location $location;
