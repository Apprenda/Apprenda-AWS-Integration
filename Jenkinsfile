node('delta') {
    currentBuild.result = "SUCCESS"
    try {
       stage 'Checkout'
            checkout scm
       stage 'Test'

       stage 'Build'
            bat 'nuget restore \"Solution Files/Apprenda-AWS/Apprenda-AWS.sln\"'
            bat "\"${tool 'MSBuild'}\" \"Solution Files/Apprenda-AWS/Apprenda-AWS.sln\" /p:Configuration=Release /p:Platform=\"Any CPU\" /p:ProductVersion=1.0.0.${env.BUILD_NUMBER}"

       stage 'Archive'
            archive 'ProjectName/bin/Release/**'
    }
    catch (err) {
        currentBuild.result = "FAILURE"
        throw err
    }
}
