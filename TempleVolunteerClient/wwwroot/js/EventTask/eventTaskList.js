var dataTable;

$(document).ready(function () {
    loadDataTable();

    $('#eventTaskTable tbody').on('click', 'span', function () {
        var data_row = dataTable.row($(this).parents('tr')).data(); // here is the change
        $("#eventTaskModal").modal('show');
        $('#eventTaskModal').on('shown.bs.modal', function () {
            $('#eventTaskId').html(data_row.eventTaskId);
            $('#eventTaskName').html(data_row.name);
            $('#eventTaskDescription').html(data_row.description);
            $('#eventTaskNote').html(data_row.note);
            $('#eventTaskIsActive').html(data_row.isActive ? "Yes" : "No");
            $('#eventTaskIsHidden').html(data_row.isActive ? "Yes" : "No");
            $('#eventTaskCreatedDate').html(data_row.createdDate);
            $('#eventTaskCreatedBy').html(data_row.createdBy);
            $('#eventTaskUpdatedDate').html(data_row.updatedDate);
            $('#eventTaskUpdatedBy').html(data_row.updatedBy);
        });
    });
});

function loadDataTable() {
    dataTable = $('#eventTaskTable').DataTable({
        "ajax": {
            "url": "/EventTask/EventTaskGet",
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
                "data": "eventTaskId",
                "render": function (data) {
                    return `<div class="text-center">
                                <span style="cursor:pointer">
                                    <img id="viewId" class='img-75' src="/img/view.png" alt="View EventTask Details" />
                                </span>
                                <a style="text-decoration:none;" href="/EventTask/Upsert?eventTaskId=${data}">
                                    <img class='img-75' src="/img/edit.png" alt="Edit EventTask" />
                                </a>
                                <a style="text-decoration:none;" href=# onclick=Delete('/EventTask/Delete?eventTaskId='+${data})>
                                    <img class='img-75' src="/img/delete.png" alt="Delete EventTask" />
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