BeforeAll {
    # Determine admin binary path based on platform
    if ($IsMacOS) {
        $script:AdminPath = Join-Path $env:InstallDirectory "Crm.Admin" "Crm.Admin"
    } else {
        $script:AdminPath = Join-Path $env:InstallDirectory "Crm.Admin" "Crm.Admin.exe"
    }

    # Verify the binary exists
    if (-not (Test-Path $script:AdminPath)) {
        throw "Admin binary not found at: $script:AdminPath. Ensure the project is built and `$env:InstallDirectory is set."
    }
}

Describe "Crm.Admin Postgres Commands" {
    Context "create-company" {
        It "should return name conflict error when creating a company with duplicate name" {
            # Generate a unique company name for this test run
            $companyName = "TestCompany_" + [guid]::NewGuid().ToString("N").Substring(0, 8)

            # First creation should succeed
            $result = & $script:AdminPath postgres create-company --name $companyName --description "Test company" 2>&1
            $firstExitCode = $LASTEXITCODE
            $firstExitCode | Should -Be 0 -Because "first company creation should succeed"

            # Second creation with same name should fail with name conflict
            $result = & $script:AdminPath postgres create-company --name $companyName --description "Duplicate test company" 2>&1
            $secondExitCode = $LASTEXITCODE
            $secondExitCode | Should -Be 1 -Because "duplicate company creation should fail"

            # Verify the error message indicates name conflict
            $errorOutput = $result | Out-String
            $errorOutput | Should -Match "Company name already exists" -Because "error message should indicate name conflict"
        }
    }
}
