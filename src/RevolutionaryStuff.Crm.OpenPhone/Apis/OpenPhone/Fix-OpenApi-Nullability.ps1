param(
  [Parameter(Mandatory=$true)][string]$In,
  [Parameter(Mandatory=$true)][string]$Out
)

function Fix-Node {
  param([Parameter(ValueFromPipeline=$true)]$node)

  if ($null -eq $node) { return }

  # Handle arrays
  if ($node -is [System.Array] -or $node -is [System.Collections.IList]) {
    for ($i = 0; $i -lt $node.Count; $i++) {
      Fix-Node $node[$i]
    }
    return
  }

  # Handle PowerShell objects (PSCustomObject)
  if ($node -is [PSCustomObject]) {
    $nodeProperties = $node.PSObject.Properties

    # Handle 'type' being an array that includes "null"
    if ($nodeProperties['type'] -and $node.type -is [System.Array]) {
      $types = @($node.type)
      $nonNullTypes = $types | Where-Object { $_ -ne 'null' }
      if ($types -contains 'null' -and $nonNullTypes.Count -eq 1) {
        $node.type = $nonNullTypes[0]
        Add-Member -InputObject $node -MemberType NoteProperty -Name 'nullable' -Value $true -Force
        Write-Host "✓ Converted type array [$($types -join ', ')] to nullable $($nonNullTypes[0])" -ForegroundColor Green
      }
    }

    # Handle anyOf/oneOf patterns
    foreach ($keyword in @('anyOf', 'oneOf')) {
      if ($nodeProperties[$keyword] -and $node.$keyword -is [System.Array]) {
        $schemaArray = $node.$keyword
        
        if ($schemaArray.Count -eq 2) {
          # Look for exactly 2 items: one with type "null" and one without
          $nullSchema = $null
          $nonNullSchema = $null
          
          foreach ($schema in $schemaArray) {
            if ($schema -is [PSCustomObject] -and $schema.PSObject.Properties['type']) {
              if ($schema.type -eq 'null') {
                $nullSchema = $schema
              } else {
                $nonNullSchema = $schema
              }
            } elseif ($schema -is [PSCustomObject]) {
              # Schema without explicit type (could be object schema, etc.)
              $nonNullSchema = $schema
            }
          }
          
          # If we found exactly one null schema and one non-null schema, convert it
          if ($nullSchema -ne $null -and $nonNullSchema -ne $null) {
            # Copy all properties from the non-null schema to the parent
            foreach ($prop in $nonNullSchema.PSObject.Properties) {
              Add-Member -InputObject $node -MemberType NoteProperty -Name $prop.Name -Value $prop.Value -Force
            }
            
            # Add nullable property and remove the anyOf/oneOf
            Add-Member -InputObject $node -MemberType NoteProperty -Name 'nullable' -Value $true -Force
            $node.PSObject.Properties.Remove($keyword)
            
            $schemaType = if ($nonNullSchema.PSObject.Properties['type']) { $nonNullSchema.type } else { 'object' }
            Write-Host "✓ Converted $keyword to nullable $schemaType" -ForegroundColor Green
          }
        }
      }
    }

    # Recursively process all child properties
    foreach ($prop in @($nodeProperties)) {
      Fix-Node $prop.Value
    }
  }
}

# Validate input file exists
if (-not (Test-Path -LiteralPath $In)) {
  Write-Error "Input file does not exist: $In"
  exit 1
}

try {
  # Load JSON
  Write-Host "Loading OpenAPI document from: $In" -ForegroundColor Cyan
  $raw = Get-Content -LiteralPath $In -Raw -ErrorAction Stop
  $obj = $raw | ConvertFrom-Json -Depth 100 -ErrorAction Stop

  # Validate OpenAPI document
  if (-not $obj.openapi) {
    Write-Warning "Input file does not appear to be an OpenAPI document (missing 'openapi' field)"
  } else {
    Write-Host "Found OpenAPI version: $($obj.openapi)" -ForegroundColor Cyan
  }

  # Count initial anyOf patterns
  $initialAnyOfCount = ($raw | Select-String '"anyOf"' -AllMatches).Matches.Count
  Write-Host "Initial anyOf patterns found: $initialAnyOfCount" -ForegroundColor Yellow

  Write-Host "`nProcessing nullable pattern conversions..." -ForegroundColor Cyan
  
  # Process the document
  Fix-Node $obj

  # Update OpenAPI version if needed
  if ($obj.openapi -and $obj.openapi -match '^3\.1\.') {
    $oldVersion = $obj.openapi
    $obj.openapi = '3.0.3'
    Write-Host "✓ Updated OpenAPI version from $oldVersion to 3.0.3" -ForegroundColor Green
  }

  # Ensure output directory exists
  $outDir = Split-Path -Path $Out -Parent
  if ($outDir -and -not (Test-Path -Path $outDir)) {
    New-Item -ItemType Directory -Path $outDir -Force | Out-Null
  }

  # Save the result
  Write-Host "`nWriting converted document to: $Out" -ForegroundColor Cyan
  $jsonOutput = $obj | ConvertTo-Json -Depth 100 -Compress:$false
  $jsonOutput | Set-Content -LiteralPath $Out -Encoding UTF8 -ErrorAction Stop

  # Count remaining anyOf patterns
  $finalAnyOfCount = ($jsonOutput | Select-String '"anyOf"' -AllMatches).Matches.Count
  $convertedCount = $initialAnyOfCount - $finalAnyOfCount
  
  Write-Host "`n📊 Conversion Summary:" -ForegroundColor White
  Write-Host "  • Initial anyOf patterns: $initialAnyOfCount" -ForegroundColor White
  Write-Host "  • Converted patterns: $convertedCount" -ForegroundColor Green
  Write-Host "  • Remaining patterns: $finalAnyOfCount" -ForegroundColor $(if ($finalAnyOfCount -eq 0) { 'Green' } else { 'Yellow' })
  
  if ($finalAnyOfCount -eq 0) {
    Write-Host "`n🎉 SUCCESS: All nullable anyOf patterns have been converted!" -ForegroundColor Green
  } else {
    Write-Host "`n⚠️  WARNING: $finalAnyOfCount anyOf patterns remain - these may need manual review" -ForegroundColor Yellow
  }
  
  Write-Host "`n✅ Successfully wrote OpenAPI 3.0 spec to: $Out" -ForegroundColor Green
} 
catch {
  Write-Error "❌ Failed to process OpenAPI document: $($_.Exception.Message)"
  Write-Error $_.Exception.StackTrace
  exit 1
}
