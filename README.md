
Apprenda-AWS-Integration
========================

This project contains all of the modules necessary to integrate with Amazon Web Services.

Release Notes
--------------

## Support Matrix
- Apprenda Cloud Platform 6.5 or later
- AWS SDK version is bundled with each stable release.
- Current fully supported AddOns: Amazon S3 and Amazon RDS

Automatic regression Tests are done against new versions of the AWS SDK, which is released on a more frequent cadence. If you need to use the latest version, build the package from source.

Installation
------------
_For full documentation on how to setup Addons on the Apprenda Cloud Platform, please view the Documentation for AWS AddOns PDF [here](https://apprenda.com/partners/integrations/amazon-web-services/)_
- Download our latest release from the Releases link at https://github.com/apprenda/Apprenda-AWS-Integration/releases
- The release will have zip files for each Add-On, which you can use to upload to Apprenda as per the next section

Usage
-----
- Upload the Add-Ons to the Apprenda Operator Portal (SOC) using instructions from http://docs.apprenda.com/8-1/addons
- Once the Add-Ons are uploaded, edit them and configure the following properties
  - Add the Location, indicating the region of AWS to provision instances of this Add-On (usually this is set to us-east-1)
  - Add the User; this is your AWS Access Key
  - Add the Password; this is your AWS Secret Access Key
  - Visit the Configuration tab of the Add-On and edit the properties to match your desired configuration for this Add-On
- As a developer, start provisioning and consuming Add-Ons as per the documentation at http://docs.apprenda.com/8-1/addonconsumption


Build From source
-----------------
- `git clone`, open .Sln in Visual Studio (or equivalent)
- `nuget restore` or from the Package Manager in Visual Studio, `Update-Package`
- If you have a private NuGet repository with the Apprenda SDK (`SaaSGrid.API`) included, you can configure it accordingly. If you do not:
  - Install the [Apprenda SDK](http://docs.apprenda.com/downloads) for the version of the Apprenda Cloud Platform you are currently running
  - Locate `SaaSGrid.API` in your installation folder (Typical installation will have it at `C:\Program Files (x86)\Apprenda\SDK\API Files`)
  - Use this path to add a new reference to `SaaSGrid.API` to each of the Visual Studio Projects in this solution
- Do a Release Build - upon success, you should have a folder in the root project directory named `Apprenda-AWS-Build` with access to each of the zip files.
- To 

Support
-------
For support on this addon, please feel free to submit an issue - we'll look into it right away.

For support regarding Apprenda Cloud platform - please contact your representative or shoot us an email at support@apprenda.com

Contributions
-------------
Are absolutely welcome, please fork and submit PRs.