var dataTable;

$(document).ready(function () {
    loadDataTable();

    $('#eventTypeTable tbody').on('click', 'span', function () {
        var data_row = dataTable.row($(this).parents('tr')).data(); // here is the change
        $("#eventTypeModal").modal('show');
        $('#eventTypeModal').on('shown.bs.modal', function () {
            $('#eventTypeId').html(data_row.eventTypeId);
            $('#eventTypeName').html(data_row.name);
            $('#eventTypeDescription').html(data_row.description);
            $('#eventTypeNote').html(data_row.note);
            $('#eventTypeIsActive').html(data_row.isActive ? "Yes" : "No");
            $('#eventTypeIsHidden').html(data_row.isActive ? "Yes" : "No");
            $('#eventTypeCreatedDate').html(data_row.createdDate);
            $('#eventTypeCreatedBy').html(data_row.createdBy);
            $('#eventTypeUpdatedDate').html(data_row.updatedDate);
            $('#eventTypeUpdatedBy').html(data_row.updatedBy);
        });
    });
});

function loadDataTable() {
    dataTable = $('#eventTypeTable').DataTable({
        "ajax": {
            "url": "/EventType/EventTypeGet",
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
                "data": "eventTypeId",
                "render": function (data) {
                    return `<div class="text-center">
                                <span style="cursor:pointer">
                                    <img id="viewId" class='img-75' src="/img/view.png" alt="View EventType Details" />
                                </span>
                                <a style="text-decoration:none;" href="/EventType/Upsert?eventTypeId=${data}">
                                    <img class='img-75' src="/img/edit.png" alt="Edit EventType" />
                                </a>
                                <a style="text-decoration:none;" href=# onclick=Delete('/EventType/Delete?eventTypeId='+${data})>
                                    <img class='img-75' src="/img/delete.png" alt="Delete EventType" />
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