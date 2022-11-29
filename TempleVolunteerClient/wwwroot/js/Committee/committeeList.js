var dataTable;

$(document).ready(function () {
    loadDataTable();

    $('#committeeTable tbody').on('click', 'span', function () {
        var data_row = dataTable.row($(this).parents('tr')).data(); // here is the change
        $("#committeeModal").modal('show');
        $('#committeeModal').on('shown.bs.modal', function () {
            $('#committeeId').html(data_row.committeeId);
            $('#committeeName').html(data_row.name);
            $('#committeeDescription').html(data_row.description);
            $('#committeeNote').html(data_row.note);
            $('#committeeIsActive').html(data_row.isActive ? "Yes" : "No");
            $('#committeeIsHidden').html(data_row.isActive ? "Yes" : "No");
            $('#committeeCreatedDate').html(data_row.createdDate);
            $('#committeeCreatedBy').html(data_row.createdBy);
            $('#committeeUpdatedDate').html(data_row.updatedDate);
            $('#committeeUpdatedBy').html(data_row.updatedBy);
        });
    });
});

function loadDataTable() {
    dataTable = $('#committeeTable').DataTable({
        "ajax": {
            "url": "/Committee/CommitteeGet",
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
                "data": "committeeId",
                "render": function (data) {
                    return `<div class="text-center">
                                <span style="cursor:pointer">
                                    <img id="viewId" class='img-75' src="/img/view.png" alt="View Committee Details" />
                                </span>
                                <a style="text-decoration:none;" href="/Committee/Upsert?committeeId=${data}">
                                    <img class='img-75' src="/img/edit.png" alt="Edit Committee" />
                                </a>
                                <a style="text-decoration:none;" href=# onclick=Delete('/Committee/Delete?committeeId='+${data})>
                                    <img class='img-75' src="/img/delete.png" alt="Delete Committee" />
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