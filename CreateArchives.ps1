$directoryPrefix = "C:\Users\mmichael\Documents\GitHub\Apprenda-AWS-Integration\Apprenda-AWS-Build\"
$directories = @("Glacier","Lambda", "RDS","Redshift", "S3", "SNS", "SQS", "AMI")

Add-Type -Assembly System.IO.Compression.FileSystem
$compressionLevel = [System.IO.Compression.CompressionLevel]::Optimal

foreach ($directoryFolder in $directories)
{
    $fullPath = $directoryPrefix + $directoryFolder    
    $zipFileResult = "$($directoryPrefix)\$($directoryFolder).zip"

    if (Test-Path $zipFileResult) 
    {
      Remove-Item $zipFileResult
    }

    [System.IO.Compression.ZipFile]::CreateFromDirectory($fullPath,
        $zipFileResult, $compressionLevel, $false)
}