<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="no-script.aspx.cs" Inherits="DataCollection.no_script" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>JavaScript is disabled :: BlackChair</title>
    <script>
        location.replace('DownloadPaperPDF.aspx');
    </script>
    <style>
        
.NoScriptMessageHolder
{
    width: 350px;
    
    margin: 10% auto;
    display: block;
    background: #ffaa11;
    padding: 20px;
    border: 3px dashed red;

}
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div>
  <noscript>
                <div style="width:100%;height:100%;position:absolute;left:0px;top:0px;color:#fff;background:#666;">
                    <div class="NoScriptMessageHolder">
                    We're sorry but you need to enable JavaScript to view Question Papers. <br /><a href="http://enable-javascript.com/" target="_blank">How to enable JavaScript?</a>
                    </div>

                </div>
            </noscript>
    
    </div>
    </form>
</body>
</html>
