<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ViewQuestionPaper.aspx.cs" Inherits="DataCollection.ViewQuestionPaper" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Display Question Paper</title>
    <link href="PaperDisplayStyle.css" rel="stylesheet" />
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.1.1/jquery.min.js"></script>
    <script src="Undownloadable.js"></script>
    <noscript>
        <meta http-equiv="refresh" content="0, url=no-script.aspx" />
    </noscript>
    
</head>
<body>

    <script src="RightClickBlocker.js"></script>
    <form id="form1" runat="server">
        <div>
      <!--
                  <asp:TextBox ID="TextBox1" runat="server" CssClass="tb" placeholder="PNR number" autofocus></asp:TextBox>

            <asp:Button ID="Button2" runat="server" Text="Generate Paper" CssClass="btn" OnClick="Button2_Click" />

            <hr />
          -->

            <noscript>
                <div style="width:100%;height:100%;position:absolute;left:0px;top:0px;color:#fff;background:#666;">
                    <div class="NoScriptMessageHolder">
                    We're sorry but you need to enable JavaScript to view papers. <br /><a href="http://enable-javascript.com/" target="_blank">How to enable JavaScript?</a>
                    </div>
                </div>
            </noscript>

            <section id="ContentSection">
                <div class="Paper" id="MessageHolder">
                <div class="SecurityMessage" id="SecurityMessage">
                       Sorry! We do not allow downloading Question Papers yet. Stay connected to start downloading papers soon.
                    <br />
                    <br />
                    <br />
                    <span id="Reload" class="Reload" onclick="new function (){location.reload();}">Click to view paper again</span>

                    </div>
                    </div>
                <asp:Panel runat="server" CssClass="Paper" ID="pnl_Paper" Visible="false">
                
                    <div class="Spreader">
                        <h1 runat="server" id ="hdoc_PaperCode" class="PaperCode" visible="false">Paper Code:&nbsp;<asp:Label ID="lbl_papercode" runat="server" Text=""></asp:Label></h1>
                        <br />
                    </div>
                    <div class="Spreader">
                        <center>
                    <span id="hdoc_regno" class="RegistrationNo">Registration No:.__________________</span><br /></center>
                    </div>
                    <div class="Spreader">
                        <span class="PNRno">PNR No:: &nbsp;<asp:Label ID="lbl_pnr" runat="server"></asp:Label></span>
                    </div>
                    <center>
                    <div class="Spreader" style="padding: 2px;">
                        <span class="CourseDetails">COURSE CODE : &nbsp;<asp:Label ID="lbl_coursecode" runat="server"></asp:Label></span>
                    </div>
                    <div class="Spreader" style="padding: 2px;">
                        <span class="CourseDetails">COURSE NAME : &nbsp;<asp:Label ID="lbl_coursename" runat="server" /></span>
                    </div></center>
                    <div class="Spreader" style="margin-top: 5px;">
                        <span class="PaperDetails PNRno" id="TimeAllowed" style="float:left;">Time Allowed: &nbsp;<asp:Label ID="lbl_timeallowed" runat="server" /></span>
                        <span class="PaperDetails PNRno" id="MaximumMarks" style="float:right;">Max.Marks: &nbsp;<asp:Label ID="lbl_maxmarks" runat="server" /></span>
                    </div>
                    <div class="Spreader">
                        
                        <p runat="server" class="PaperInstructions" id="InstFOsholo">
                            Read the following instructions carefully before attempting the question paper.<br />
1. &nbsp;&nbsp;This question paper is divided into two parts A and B.<br />
2.  &nbsp;&nbsp; Part A contains &nbsp;<asp:Label ID="lbl_npartA" runat="server" /> questions of &nbsp;<asp:Label ID="lbl_wpartA" runat="server" /> marks each. All questions are compulsary.<br />
 3. &nbsp;&nbsp;Part B contains &nbsp;<asp:Label ID="lbl_npartB" runat="server" /> questions of &nbsp;<asp:Label ID="lbl_wpartB" runat="server" /> marks each. In each question attempt either question (a) or (b), in case both (a)<br />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;and
(b) questions are attempted for any question then only the first attempted question will be evaluated.<br />
 4.&nbsp;&nbsp; Answer all questions in serial order.<br />
 5. &nbsp;&nbsp;Do not write or mark anything on the question paper except your registration no. on the designated space.<br />
                        </p>
                        <p runat="server" class="PaperInstructions" id="InstFOob">
                            Read the following instructions carefully before attempting the question paper.<br />
1. &nbsp;&nbsp;Match the Paper Code shaded on the OMR Sheet with the Paper Code mentioned on the question paper and ensure<br />&nbsp;&nbsp; &nbsp;&nbsp; that both are the same.<br />
2. &nbsp; Part A contains &nbsp;<asp:Label ID="lbl_npartA1" runat="server" /> questions of &nbsp;<asp:Label ID="lbl_wpartA1" runat="server" /> marks each. All questions are compulsary. 0.25 marks will be deducted for each <br />&nbsp;&nbsp; &nbsp;&nbsp;&nbsp;wrong answer.<br />
 3. &nbsp;&nbsp;Part B contains &nbsp;<asp:Label ID="lbl_npartB1" runat="server" /> questions of &nbsp;<asp:Label ID="lbl_wpartB1" runat="server" /> marks each. In each question attempt either question (a) or (b), in case both (a)<br />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; and (b) questions are attempted for any question then only the first attempted question will be evaluated.<br />
 4.&nbsp;&nbsp; Answer all questions in serial order.<br />
 5. &nbsp;&nbsp;Do not write or mark anything on the question paper except your registration no. on the designated space.<br />
                        </p>
                        <p runat="server" class="PaperInstructions" id="InstFoObonly">
                            Read the following instructions carefully before attempting the question paper.<br />
1. &nbsp;&nbsp;Match the Paper Code shaded on the OMR Sheet with the Paper Code mentioned on the question paper and ensure<br />&nbsp;&nbsp; &nbsp;&nbsp; that both are the same.<br />
2. &nbsp; This question paper contains &nbsp;<asp:Label ID="lbl_QcountOB" runat="server" /> questions of &nbsp;<asp:Label ID="lbl_QwtOB" runat="server" /> each. All questions are compulsary. 0.25 marks will be deducted &nbsp;&nbsp; &nbsp;&nbsp;&nbsp;for each wrong answer.<br />
 3.&nbsp;&nbsp; Answer all questions in serial order.<br />
 4. &nbsp;&nbsp;Do not write or mark anything on the question paper except your registration no. on the designated space.<br />
                            5. &nbsp;&nbsp;Submit the question paper and the rought sheet(s) along with the OMR sheet to the invigilator before leaving the
examination hall.
                        </p>
                    </div>
                   
                    <asp:Panel runat="server" ID="pnl_QuestionsHolder"></asp:Panel>
                        
                </asp:Panel>
                
                <asp:Panel ID="pnl_CaPaper" CssClass="Paper" runat="server" Visible="false"></asp:Panel>
                
            </section>

        </div>
<center><small style="font-size: .8em;margin-top: 10px;">BlackChair</small>   </center>
    </form>
</body>
</html>
