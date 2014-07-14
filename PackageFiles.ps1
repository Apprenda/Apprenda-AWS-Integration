$zipfilename=$args[0]
$sourcedir=$args[1]

function ZipFiles( $zipfilename, $sourcedir )
{
   If (Test-Path $zipfileName){
	Remove-Item $zipfileName
    }

   Add-Type -Assembly System.IO.Compression.FileSystem
   $compressionLevel = [System.IO.Compression.CompressionLevel]::Optimal
   [System.IO.Compression.ZipFile]::CreateFromDirectory($sourcedir,
        $zipfilename, $compressionLevel, $false)
}

ZipFiles $zipfilename $sourcedir