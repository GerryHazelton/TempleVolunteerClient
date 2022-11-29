var dataTable;

$(document).ready(function () {
    loadDataTable();

    $('#roleTable tbody').on('click', 'span', function () {
        var data_row = dataTable.row($(this).parents('tr')).data(); // here is the change
        $("#roleModal").modal('show');
        $('#roleModal').on('shown.bs.modal', function () {
            $('#roleId').html(data_row.roleId);
            $('#roleName').html(data_row.name);
            $('#roleDescription').html(data_row.description);
            $('#roleNote').html(data_row.note);
            $('#roleIsActive').html(data_row.isActive ? "Yes" : "No");
            $('#roleIsHidden').html(data_row.isActive ? "Yes" : "No");
            $('#roleCreatedDate').html(data_row.createdDate);
            $('#roleCreatedBy').html(data_row.createdBy);
            $('#roleUpdatedDate').html(data_row.updatedDate);
            $('#roleUpdatedBy').html(data_row.updatedBy);
        });
    });
});

function loadDataTable() {
    dataTable = $('#roleTable').DataTable({
        "ajax": {
            "url": "/Role/RoleGet",
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
                "data": "roleId",
                "render": function (data) {
                    return `<div class="text-center">
                                <span style="cursor:pointer">
                                    <img id="viewId" class='img-75' src="/img/view.png" alt="View Role Details" />
                                </span>
                                <a style="text-decoration:none;" href="/Role/Upsert?roleId=${data}">
                                    <img class='img-75' src="/img/edit.png" alt="Edit Role" />
                                </a>
                                <a style="text-decoration:none;" href=# onclick=Delete('/Role/Delete?roleId='+${data})>
                                    <img class='img-75' src="/img/delete.png" alt="Delete Role" />
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