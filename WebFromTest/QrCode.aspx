<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="QrCode.aspx.cs" Inherits="WebFromTest.QrCode" %>
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

    <script src="PageJs/QrCode.js"></script>
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
        <div class="row no-gutters">
            <div class="col-md-6">
                <div id="reader" class="border border-danger">
                </div>
            </div>
            <div class="col-md-6 border border-primary" id="ownReader">
                <div class="d-flex justify-content-center">
                    <div class="" style="max-width: 300px" id="pic">
                    </div>
                </div>

                <div>
                    <select class="form-control form-control-lg" id="cameraList">
                        <option value="-1" selected>Choose...</option>
                    </select>
                    <input type="file" id="qr-input-file" accept="image/*" capture>
                    <button id="btnScanQRCode" type="button" class="btn btn-primary btn-sm">掃描QR Code</button>
                    <button id="btnStopScan" type="button" class="btn btn-primary btn-sm">停止QR Code</button>
                    <p id="qrCodeText"></p>
                </div>
            </div>
        </div>
    </div>
</body>
</html>
