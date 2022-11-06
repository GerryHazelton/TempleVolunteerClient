var dataTable;

$(message).ready(function () {
    loadDataTable();
    debugger;
    $('#messageTable tbody').on('click', 'span', function () {
        var data_row = dataTable.row($(this).parents('tr')).data(); // here is the change
        $("#messageModal").modal('show');
        $('#messageModal').on('shown.bs.modal', function () {
            $('#messageName').html(data_row.name);
            $('#messageDescription').html(data_row.description);
            $('#messageNote').html(data_row.note);
            $('#messageIsActive').html(data_row.isActive ? "Yes" : "No");
            $('#messageCreatedDate').html(data_row.createdDate);
            $('#messageCreatedBy').html(data_row.createdBy);
            $('#messageUpdatedDate').html(data_row.updatedDate);
            $('#messageUpdatedBy').html(data_row.updatedBy);
        });
    });
});

function loadDataTable() {
    dataTable = $('#messageTable').DataTable({
        "ajax": {
            "url": "/Message/MessageGet",
            "type": "GET",
            "datatype": "json",
            "serverSide": false,
            "error": function () {
                window.location.href = "/Account/MessageModalPopUp"; 
            }
        },
        "columns": [
            { "data": "name", "width": "10%" },
            { "data": "description", "width": "10%" },
            {
                "data": "messageId",
                "render": function (data) {
                    return `<div class="text-center">
                                <span style="cursor:pointer">
                                    <img id="viewId" class='img-75' src="/img/view.png" alt="View Message Details" />
                                </span>
                                <a href="/Message/MessageUpsert?messageId=${data}">
                                    <img class='img-75' src="/img/edit.png" alt="Edit Message" />
                                </a>
                                <a href=# onclick=Delete('/Message/MessageDelete?messageId='+${data})>
                                    <img class='img-75' src="/img/delete.png" alt="Delete Message" />
                                </a>
                            </div>`;
                }, "width": "40%"
            }
        ],
        "language": {
            "emptyTable": "no data found"
        },
        "width": "100%"
    });
}

function Delete(url) {
    swal({
        title: "Are you sure?",
        text: "Once deleted, you will not be able to un-delete, continue?",
        icon: "warning",
        buttons: true,
        dangerMode: true
    }).then((willDelete) => {
        if (willDelete) {
            $.ajax({
                type: "DELETE",
                url: url,
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                        dataTable.ajax.reload();
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            });
        }
    });
}