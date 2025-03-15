$location = Get-Location;
Set-Location $PSScriptRoot;

if(test-Path $PSScriptRoot\Migrations){
	$count = (Get-Childitem $PSScriptRoot\Migrations\ -recurse -filter *_SamplePostgresMigration_v*.Designer.cs  | Measure-Object).Count + 1;
}else{
	$count = 1;
}

$name = "v$count";
"Creating migration $name";
dotnet ef migrations add SamplePostgresMigration_$name --context SamplePostgresMigration --output-dir (Get-Path Migrations, Postgres)

"PreMigrationScripts";
dotnet run -- postgres exec-script $PSScriptRoot\PreMigrationScripts;

"Migrate";
dotnet run -- postgres ef-migrate

"PostMigrationScripts";
dotnet run -- postgres exec-script $PSScriptRoot\Scripts

"Generate scripts"
dotnet run -- create-sql-script --output-file $PSScriptRoot\db\tables.sql --drop-script $PSScriptRoot\db\drop.sql

Set-Location $location;
