$location = Get-Location;
Set-Location $PSScriptRoot;

if(test-Path $PSScriptRoot\Migrations){
	$count = (Get-Childitem $PSScriptRoot\Migrations\ -recurse -filter *_SamplePostgresMigration_v*.Designer.cs  | Measure-Object).Count + 1;
}else{
	$count = 1;
}

$name = "v$count";
"Creating migration $name";
dotnet ef migrations add SamplePostgresMigration_$name --context SamplePostgresMigration --output-dir (Join Migrations, Postgres)

"PreMigrationScripts";
dotnet run -- postgres exec-script $PSScriptRoot\PreMigrationScripts;

"Migrate";
dotnet run --no-build -- postgres ef-migrate

"PostMigrationScripts";
dotnet run --no-build -- postgres exec-script $PSScriptRoot\Scripts

"Generate scripts"
dotnet run --no-build -- postgres create-script --output-file $PSScriptRoot\db\tables.sql --drop-script $PSScriptRoot\db\drop.sql
dotnet run --no-build -- postgres create-script --output-file (Join $PSScriptRoot, "db", "postgres", "tables.sql") --drop-script (Join $PSScriptRoot, "db", "postgres", "drop.sql")

Set-Location $location;
