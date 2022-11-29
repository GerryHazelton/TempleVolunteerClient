var dataTable;

$(document).ready(function () {
    loadDataTable();

    $('#categoryTable tbody').on('click', 'span', function () {
        var data_row = dataTable.row($(this).parents('tr')).data(); // here is the change
        $("#categoryModal").modal('show');
        $('#categoryModal').on('shown.bs.modal', function () {
            $('#categoryId').html(data_row.categoryId);
            $('#categoryName').html(data_row.name);
            $('#categoryDescription').html(data_row.description);
            $('#categoryNote').html(data_row.note);
            $('#categoryIsActive').html(data_row.isActive ? "Yes" : "No");
            $('#categoryIsHidden').html(data_row.isActive ? "Yes" : "No");
            $('#categoryCreatedDate').html(data_row.createdDate);
            $('#categoryCreatedBy').html(data_row.createdBy);
            $('#categoryUpdatedDate').html(data_row.updatedDate);
            $('#categoryUpdatedBy').html(data_row.updatedBy);
        });
    });
});

function loadDataTable() {
    dataTable = $('#categoryTable').DataTable({
        "ajax": {
            "url": "/Category/CategoryGet",
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
                "data": "categoryId",
                "render": function (data) {
                    return `<div class="text-center">
                                <span style="cursor:pointer">
                                    <img id="viewId" class='img-75' src="/img/view.png" alt="View Category Details" />
                                </span>
                                <a style="text-decoration:none;" href="/Category/Upsert?categoryId=${data}">
                                    <img class='img-75' src="/img/edit.png" alt="Edit Category" />
                                </a>
                                <a style="text-decoration:none;" href=# onclick=Delete('/Category/Delete?categoryId='+${data})>
                                    <img class='img-75' src="/img/delete.png" alt="Delete Category" />
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