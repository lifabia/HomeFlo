﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="flogin.aspx.cs" Inherits="webLoveCasale.flogin" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:Login ID="pnLogin" runat="server" OnAuthenticate="pnLogin_Authenticate">
            </asp:Login>
        </div>
    </form>
</body>
</html>
