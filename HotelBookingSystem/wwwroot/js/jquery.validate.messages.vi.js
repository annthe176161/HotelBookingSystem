/*
 * Translated default messages for the jQuery validation plugin.
 * Locale: VI (Vietnamese)
 */
(function ($) {
    $.extend($.validator.messages, {
        required: "Vui lòng nhập thông tin này.",
        remote: "Vui lòng kiểm tra lại thông tin này.",
        email: "Vui lòng nhập địa chỉ email hợp lệ.",
        url: "Vui lòng nhập URL hợp lệ.",
        date: "Vui lòng nhập ngày hợp lệ.",
        dateISO: "Vui lòng nhập ngày hợp lệ (ISO).",
        number: "Vui lòng nhập số hợp lệ.",
        digits: "Vui lòng chỉ nhập chữ số.",
        creditcard: "Vui lòng nhập số thẻ tín dụng hợp lệ.",
        equalTo: "Vui lòng nhập giá trị giống như trên.",
        accept: "Vui lòng nhập giá trị với định dạng hợp lệ.",
        maxlength: $.validator.format("Vui lòng nhập không quá {0} ký tự."),
        minlength: $.validator.format("Vui lòng nhập ít nhất {0} ký tự."),
        rangelength: $.validator.format("Vui lòng nhập từ {0} đến {1} ký tự."),
        range: $.validator.format("Vui lòng nhập giá trị từ {0} đến {1}."),
        max: $.validator.format("Vui lòng nhập giá trị nhỏ hơn hoặc bằng {0}."),
        min: $.validator.format("Vui lòng nhập giá trị lớn hơn hoặc bằng {0}."),
        step: $.validator.format("Vui lòng nhập bội số của {0}.")
    });
}(jQuery));
