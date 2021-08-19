<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="test1.aspx.cs" Inherits="WebFromTest.test1" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no">
    <title></title>
    <%--jquery--%>
    <script src="js/bootstrap/js/jquery-3.6.0.min.js"></script>

    <%--bootstrap--%>
    <link href="PluginJs/bootstrap/css/bootstrap.min.css" rel="stylesheet" />
    <script src="PluginJs/bootstrap/js/bootstrap.bundle.min.js"></script>

    <%--jquery-ui--%>
    <link href="PluginJs/jquery-ui/jquery-ui.min.css" rel="stylesheet" />
    <link href="PluginJs/jquery-ui/jquery-ui.theme.min.css" rel="stylesheet" />
    <script src="PluginJs/jquery-ui/jquery-ui.min.js"></script>

    <%--qrcode--%>
    <script src="PluginJs/qrcode/html5-qrcode.min.js"></script>

    <script src="PageJs/test1.js"></script>
    <style>
        html, body {
            height: 100%;
            margin: 0;
            padding: 0;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    </form>
    <div class="container-fluid" style="">
        <input id="tags" class="form-control">
    </div>

</body>
</html>
