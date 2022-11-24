var dataTable;

$(document).ready(function () {
    loadDataTable();

    $('#documentTable tbody').on('click', 'span', function () {
        var data_row = dataTable.row($(this).parents('tr')).data(); // here is the change
        $("#documentModal").modal('show');
        $('#documentModal').on('shown.bs.modal', function () {
            $('#documentId').html(data_row.documentId);
            $('#documentName').html(data_row.name);
            $('#documentDescription').html(data_row.description);
            $('#documentNote').html(data_row.note);
            $('#documentIsActive').html(data_row.isActive ? "Yes" : "No");
            $('#documentIsHidden').html(data_row.isHidden ? "Yes" : "No");
            $('#documentCreatedDate').html(data_row.createdDate);
            $('#documentCreatedBy').html(data_row.createdBy);
            $('#documentUpdatedDate').html(data_row.updatedDate);
            $('#documentUpdatedBy').html(data_row.updatedBy);
        });
    });
});

function loadDataTable() {
    dataTable = $('#documentTable').DataTable({
        "ajax": {
            "url": "/Document/DocumentGet",
            "type": "GET",
            "datatype": "json",
            "serverSide": false,
            "error": function () {
                window.location.href = "/Account/DocumentModalPopUp";
            }
        },
        "columns": [
            { "data": "name", "width": "10%" },
            { "data": "description", "width": "10%" },
            { "data": "note", "width": "10%" },
            {
                "data": "documentId",
                "render": function (data) {
                    return `<div class="text-center">
                                <span style="cursor:pointer">
                                    <img id="viewId" class='img-75' src="/img/view.png" alt="View Document Details" />
                                </span>
                                <a style="text-decoration:none;" href="/Document/Upsert?documentId=${data}">
                                    <img class='img-75' src="/img/edit.png" alt="Edit Document" />
                                </a>
                                <a style="text-decoration:none;" href=# onclick=Delete('/Document/Delete?documentId='+${data})>
                                    <img class='img-75' src="/img/delete.png" alt="Delete Document" />
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