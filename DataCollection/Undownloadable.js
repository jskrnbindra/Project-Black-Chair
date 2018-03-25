var isOpera = (!!window.opr && !!opr.addons) || !!window.opera || navigator.userAgent.indexOf(' OPR/') >= 0;
var isFirefox = typeof InstallTrigger !== 'undefined';
var isSafari = Object.prototype.toString.call(window.HTMLElement).indexOf('Constructor') > 0;
var isIE = /*@cc_on!@*/false || !!document.documentMode;
var isEdge = !isIE && !!window.StyleMedia;
var isChrome = !!window.chrome && !!window.chrome.webstore;
var isBlink = (isChrome || isOpera) && !!window.CSS;

$(document).on('keydown', function (e) {
    if (e.ctrlKey && (e.key == "s" || e.key == "u")) {

        
        $("#pnl_CaPaper").hide();
        $("#pnl_Paper").hide();
        $("#MessageHolder").show();

        e.cancelBubble = true;
        e.preventDefault();
        e.defaultPrevented();

        e.stopImmediatePropagation();
        alert("Downloading this document is not allowed at the moment.");

    }
});

if (isChrome == true) {
    if ('matchMedia' in window) {
        window.matchMedia('print').addListener(function (media) {
            if (media.matches) {
                beforePrint();
            }
            else {
                $(document).one('mouseover', afterPrint);
            }
        });
    }
    else {
        $(window).on('beforeprint', beforePrint);
        $(window).on('afterprint', afterPrint);
    }

}
else {

    $(document).on('keydown', function (e) {
        if (e.ctrlKey && (e.key == "p" || e.charCode == 16 || e.charCode == 112 || e.keyCode == 80 || e.key == "s")) {

            $("#pnl_CaPaper").hide();
            $("#pnl_Paper").hide();
            $("#MessageHolder").show();

            alert("Downloading this document is not allowed at the moment.");

            e.cancelBubble = true;
            e.preventDefault();
            e.defaultPrevented();


            e.stopImmediatePropagation();
        }
    });
}

function beforePrint() {
    $("#pnl_CaPaper").hide();
    $("#pnl_Paper").hide();
    $("#MessageHolder").show();
}

function afterPrint() {
    $("#pnl_CaPaper").show();
    $("#pnl_Paper").show();
    $("#MessageHolder").hide();
}