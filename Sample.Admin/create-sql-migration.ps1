$location = Get-Location;
Set-Location $PSScriptRoot;

if(test-Path $PSScriptRoot\Migrations){
	$count = (Get-Childitem $PSScriptRoot\Migrations\ -recurse -filter *_SampleSqlServerMigration_v*.Designer.cs  | Measure-Object).Count + 1;
}else{
	$count = 1;
}

$name = "v$count";
"Creating migration $name";
dotnet ef migrations add SampleSqlServerMigration_$name --context SampleSqlServerMigration --output-dir (Get-Path Migrations, SqlServer)

"PreMigrationScripts";
dotnet run -- sql exec-script $PSScriptRoot\PreMigrationScripts;

"Migrate";
dotnet run --no-build -- sql ef-migrate

"PostMigrationScripts";
dotnet run --no-build -- sql exec-script $PSScriptRoot\Scripts

"Generate scripts"
dotnet run --no-build -- sql create-script --output-file $PSScriptRoot\db\tables.sql --drop-script $PSScriptRoot\db\drop.sql

Set-Location $location;
