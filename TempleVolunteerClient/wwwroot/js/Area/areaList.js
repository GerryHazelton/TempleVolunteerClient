var dataTable;

$(document).ready(function () {
    loadDataTable();

    $('#areaTable tbody').on('click', 'span', function () {
        var data_row = dataTable.row($(this).parents('tr')).data(); // here is the change
        $("#areaModal").modal('show');
        $('#areaModal').on('shown.bs.modal', function () {
            $('#areaId').html(data_row.areaId);
            $('#areaName').html(data_row.name);
            $('#areaDescription').html(data_row.description);
            $('#areaNote').html(data_row.note);
            $('#areaIsActive').html(data_row.isActive ? "Yes" : "No");
            $('#areaIsHidden').html(data_row.isActive ? "Yes" : "No");
            $('#areaCreatedDate').html(data_row.createdDate);
            $('#areaCreatedBy').html(data_row.createdBy);
            $('#areaUpdatedDate').html(data_row.updatedDate);
            $('#areaUpdatedBy').html(data_row.updatedBy);
        });
    });
});

function loadDataTable() {
    dataTable = $('#areaTable').DataTable({
        "ajax": {
            "url": "/Area/AreaGet",
            "type": "GET",
            "datatype": "json",
            "serverSide": false,
            "error": function () {
                window.location.href = "/Account/ErrorModalPopUp"; 
            }
        },
        "columns": [
            { "data": "name", "width": "10%" },
            { "data": "description", "width": "10%" },
            { "data": "note", "width": "10%" },
            {
                "data": "areaId",
                "render": function (data) {
                    return `<div class="text-center">
                                <span style="cursor:pointer">
                                    <img id="viewId" class='img-75' src="/img/view.png" alt="View Area Details" />
                                </span>
                                <a style="text-decoration:none;" href="/Area/Upsert?areaId=${data}">
                                    <img class='img-75' src="/img/edit.png" alt="Edit Area" />
                                </a>
                                <a style="text-decoration:none;" href=# onclick=Delete('/Area/Delete?areaId='+${data})>
                                    <img class='img-75' src="/img/delete.png" alt="Delete Area" />
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