function toggleSpinner(targetName, isShow = false) {
    let spinner = $('.element-spinner[data-target="' + targetName + '"]');
    if (isShow)
        spinner.show();
    else
        spinner.hide();
}

function toggleReloader(targetName, isShow, callback) {
    let spinner = $('.element-spinner[data-target="' + targetName + '"]');
    spinner.hide();
    let colspan = $('#table-data thead tr').children().length;
    $('#table-data tbody').html('<tr><td class="text-center" colspan="'+colspan+'">Gagal memuat data. Cek koneksi.<br><a href="javascript:void(0)" style="text-decoration:underline">Muat ulang</a>.</td></tr>');
    $('a', $('#table-data td'))[0].onclick = function () {
        callback();
        spinner.show();
        $('#table-data td').html('<tr><td class="text-center" colspan="' + colspan +'">Sedang memuat data ...</td></tr>');
    }
}

function CustomDataTable(options) {

    let _options = {
        url: '',
        extraParam: '',
        container: null,
        scrollTarget: null,
        onRender: function () { },
        onDataAvailable: function () { },
    };

    for (let key in _options) {
        if (typeof (options[key]) != 'undefined')
            _options[key] = options[key];
    }

    function displayTableItem(data) {
        let table = _options.container;
        table.html('');
        let tableRows = document.createDocumentFragment();
        for (let item of data) {
            let templateNode = _options.onRender(item);
            tableRows.append(templateNode);
        }
        if (data.length === 0) {
            let template = $('#tmp-item-row-default')[0].content.cloneNode(true);
            tableRows.append(template);
        }
        table.append(tableRows);
    }

    function getListData(pageNumber = 1) {
        $.ajax({
            url: _options.url + `?page=${pageNumber}` + _options.extraParam,
            beforeSend: () => toggleSpinner('table', true),
            complete: (e) => { if (e.status === 200) toggleSpinner('table', false) },
            error: () => toggleReloader('table', true, () => getListData(pageNumber)),
            success: function (result) {
                displayTableItem(result.data.items);
                let pageSize = result.data.pageSize;
                let totalItem = result.data.totalItemsCount;
                updatePagination(totalItem, pageSize, pageNumber);
                fixPageStyle();
                _options.onDataAvailable(result.data);
            }
        });
    }

    function fixPageStyle() {
        $('.right_col')[0].style.minHeight = window.innerHeight + 'px';
    }

    function updatePagination(totalItem = 1, pageSize = 1, pageNumber = 1) {
        let disallowSelectOnLoad = true;
        $(".pagination").paging(totalItem, {
            format: '[nnncnnn]',
            perpage: pageSize,
            lapping: 0,
            page: pageNumber,
            onSelect: function (page) {
                if (disallowSelectOnLoad) {
                    disallowSelectOnLoad = false;
                } else {
                    $('html, body').animate({ scrollTop: _options.scrollTarget.offset().top }, 250);
                    getListData(page);
                }
            },
            onFormat: function (type) {
                switch (type) {
                    case 'block':
                        if (this.value == pageNumber)
                            return '<li class="page-item active"><span class="page-link">' + this.value + '</span></li>';
                        else
                            return '<li><a href="#" class="page-link">' + this.value + '</a></li>';
                    case 'first':
                        return '<li><a href="#" class="page-link" title="First Page">«</a></li>';
                    case 'last':
                        return '<li><a href="#" class="page-link" title="Last Page">»</a></li>';
                }
            }
        });
    }

    function init() {
        getListData();
    }

    return {
        init,
    };
}

function reformatDate(date) {
    let d = new Date(date);
    return ('0' + d.getDate()).slice(-2) + '/' + ('0' + (d.getMonth() + 1)).slice(-2) + '/' + d.getFullYear() + ' ' + d.toTimeString().split(' ')[0];
}