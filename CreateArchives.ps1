$directoryPrefix = "C:\Users\mmichael\Documents\GitHub\Apprenda-AWS-Integration\Apprenda-AWS-Build\"
#$directories = @("Glacier","Lambda", "RDS","Redshift", "S3", "SNS", "SQS", "AMI")
$directories = @("RDS","S3")

Add-Type -Assembly System.IO.Compression.FileSystem
$compressionLevel = [System.IO.Compression.CompressionLevel]::Optimal

foreach ($directoryFolder in $directories)
{
    $fullPath = $directoryPrefix + $directoryFolder    
    $zipFileResult = "$($directoryPrefix)\Apprenda.AddOn.Amazon.AWS.$($directoryFolder).zip"

    if (Test-Path $zipFileResult) 
    {
      Remove-Item $zipFileResult
    }

    [System.IO.Compression.ZipFile]::CreateFromDirectory($fullPath,
        $zipFileResult, $compressionLevel, $false)
}