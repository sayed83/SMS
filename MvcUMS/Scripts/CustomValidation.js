   // jQuery ".Class" SELECTOR.
    $(document).ready(function () {

        $(".Numeric").on("keypress keyup blur", function (event) {
            //this.value = this.value.replace(/[^0-9\.]/g,'');
            $(this).val($(this).val().replace(/[^0-9\.]/g, ''));
            if ((event.which != 46 || $(this).val().indexOf('.') != -1) && (event.which < 48 || event.which > 57)) {
                event.preventDefault();
            }
        });


        var showChar = 55;
        var ellipsestext = "...";
        var moretext = "more";
        var lesstext = "less";
        $('.more').each(function () {
            var content = $(this).html();

            if (content.length > showChar) {

                var c = content.substr(0, showChar);
                var h = content.substr(showChar - 1, (content.length - showChar)+1);

                var html = c + '<span class="moreellipses">' + ellipsestext + '&nbsp;</span><span class="morecontent"><span>' + h + '</span>&nbsp;&nbsp;<a href="" class="morelink">' + moretext + '</a></span>';

                $(this).html(html);
            }

        });

        $(".morelink").click(function () {
            if ($(this).hasClass("less")) {
                $(this).removeClass("less");
                $(this).html(moretext);
            } else {
                $(this).addClass("less");
                $(this).html(lesstext);
            }
            $(this).parent().prev().toggle();
            $(this).prev().toggle();
            return false;
        });

    });

    $(".Percentage").keyup(function () {
        if (parseInt($(this).val()) > 100) {
            alert("Please input should be a percentage")
            $(this).val("")
            $(this).focus()
        }
    })

    $(".Date").datepicker(
                   {
                       dateFormat: 'yy/mm/dd',
                       changeMonth: true,
                       changeYear: true,
                       yearRange: "-5:+5"
                   });


    $(document).ready(function () {
        
    });