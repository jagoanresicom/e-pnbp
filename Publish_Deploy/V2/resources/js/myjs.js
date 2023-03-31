﻿(function (a) { a.createModal = function (b) { defaults = { title: "", message: "Your Message Goes Here!", closeButton: true, scrollable: false }; var b = a.extend({}, defaults, b); var c = (b.scrollable === true) ? 'style="max-height: 420px;overflow-y: auto;"' : ""; html = '<div class="modal fade" id="myModal">'; html += '<div class="modal-dialog">'; html += '<div class="modal-content">'; html += '<div class="modal-header">'; html += '<button type="button" class="close" data-dismiss="modal" aria-hidden="true">×</button>'; if (b.title.length > 0) { html += '<h4 class="modal-title">' + b.title + "</h4>" } html += "</div>"; html += '<div class="modal-body" ' + c + ">"; html += b.message; html += "</div>"; html += '<div class="modal-footer">'; if (b.closeButton === true) { html += '<button type="button" class="btn btn-primary" data-dismiss="modal" id="btn-ok">OK</button>' } html += "</div>"; html += "</div>"; html += "</div>"; html += "</div>"; a("body").prepend(html); a("#myModal").modal().on("hidden.bs.modal", function () { a(this).remove() }) } })(jQuery);
function showmsg(judul, isi) { $.createModal({ title: judul, message: isi, closeButton: true, scrollable: false }); return false; };
var formatangka = function (data, type, row) {
    return "".concat(data).toString().replace(/\B(?=(\d{3})+(?!\d))/g, ".");
};

function showalert(content) {
    swal({
        title: 'Perhatian',
        text: content,
        confirmButtonText: 'OK',
        confirmButtonColor: "#f2837b"
    });
    return false;
};

function showinfo(content) {
    swal({
        title: 'Informasi',
        text: content,
        confirmButtonText: 'OK',
        confirmButtonColor: "#2b982b"
    });
    return false;
};