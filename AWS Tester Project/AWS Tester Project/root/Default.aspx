<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="root._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Interface</title>
</head>
<body>
    <form id="form1" runat="server">
    Hello,
    <asp:Label ID="nameLabel" runat="server" />!
    <br />
    <asp:Label ID="statusMessage" runat="server" ForeColor="Red" />
    <br />
    <asp:Button ID="serviceCallButton" runat="server" Text="Call Service" OnClick="OnServiceCallButtonClick" />
    </form>
</body>
</html>
