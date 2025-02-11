# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# Integration tests for module cmdlets
#

[CmdletBinding()]
param ()

BeforeAll {
    # Setup error handling
    $ErrorActionPreference = 'Stop';
    Set-StrictMode -Version latest;

    if ($Env:SYSTEM_DEBUG -eq 'true') {
        $VerbosePreference = 'Continue';
    }

    # Setup tests paths
    $rootPath = $PWD;
    Import-Module (Join-Path -Path $rootPath -ChildPath out/modules/PSRule.Rules.GitHub) -Force;
    $outputPath = Join-Path -Path $rootPath -ChildPath out/tests/PSRule.Rules.GitHub.Tests/Cmdlet;
    Remove-Item -Path $outputPath -Force -Recurse -Confirm:$False -ErrorAction Ignore;
    $Null = New-Item -Path $outputPath -ItemType Directory -Force;
}

#region Export-GitHubRuleData

Describe 'Export-GitHubRuleData' -Tag 'Cmdlet','Export-GitHubRuleData' {
    Context 'With defaults' {
        It 'Exports repository data' {
            $results = @(Export-GitHubRuleData -OutputPath $outputPath -R Microsoft/PSRule,Microsoft/PSRule.Rules.GitHub);
            $results | Should -Not -BeNullOrEmpty;
            $results | Should -BeOfType System.IO.FileInfo;
            $jsonResults = Get-Content -Path $results.FullName -Raw | ConvertFrom-Json;

            # Get repository
            $filteredResults = @($jsonResults | Where-Object { $_.Type -eq 'api.github.com/repos' });
            $filteredResults | Should -Not -BeNullOrEmpty;
            $filteredResults.Length | Should -Be 2;
            $filteredResults.Name | Should -BeIn 'PSRule', 'PSRule.Rules.GitHub';
            $filteredResults.Owner | Should -BeIn 'microsoft', 'BernieWhite';
            $filteredResults.DefaultBranch | Should -BeIn 'main';
            $filteredResults.Branches | Should -Not -BeNullOrEmpty;
            $filteredResults.Branches.Length | Should -BeGreaterOrEqual 3;

            # Get branches
            $filteredResults = @($jsonResults | Where-Object { $_.Type -eq 'api.github.com/repos/branches' });
            $filteredResults | Should -Not -BeNullOrEmpty;
            $filteredResults.Length | Should -BeGreaterOrEqual 3;
        }
    }
}

#endregion Export-GitHubRuleData
