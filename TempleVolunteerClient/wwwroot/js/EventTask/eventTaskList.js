var dataTable;

$(eventTask).ready(function () {
    loadDataTable();
    debugger;
    $('#eventTaskTable tbody').on('click', 'span', function () {
        var data_row = dataTable.row($(this).parents('tr')).data(); // here is the change
        $("#eventTaskModal").modal('show');
        $('#eventTaskModal').on('shown.bs.modal', function () {
            $('#eventTaskName').html(data_row.name);
            $('#eventTaskDescription').html(data_row.description);
            $('#eventTaskNote').html(data_row.note);
            $('#eventTaskIsActive').html(data_row.isActive ? "Yes" : "No");
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
                window.location.href = "/Account/EventTaskModalPopUp"; 
            }
        },
        "columns": [
            { "data": "name", "width": "10%" },
            { "data": "description", "width": "10%" },
            {
                "data": "eventTaskId",
                "render": function (data) {
                    return `<div class="text-center">
                                <span style="cursor:pointer">
                                    <img id="viewId" class='img-75' src="/img/view.png" alt="View EventTask Details" />
                                </span>
                                <a href="/EventTask/EventTaskUpsert?eventTaskId=${data}">
                                    <img class='img-75' src="/img/edit.png" alt="Edit EventTask" />
                                </a>
                                <a href=# onclick=Delete('/EventTask/EventTaskDelete?eventTaskId='+${data})>
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