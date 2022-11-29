var dataTable;

$(document).ready(function () {
    loadDataTable();

    $('#eventTable tbody').on('click', 'span', function () {
        var data_row = dataTable.row($(this).parents('tr')).data(); // here is the change
        $("#eventModal").modal('show');
        $('#eventModal').on('shown.bs.modal', function () {
            $('#eventId').html(data_row.eventId);
            $('#eventName').html(data_row.name);
            $('#eventDescription').html(data_row.description);
            $('#eventNote').html(data_row.note);
            $('#eventStartDate').html(data_row.startDate);
            $('#eventEndDate').html(data_row.endDate);
            $('#eventIndefinite').html(data_row.indifinite);
            $('#eventIsActive').html(data_row.isActive ? "Yes" : "No");
            $('#eventIsHidden').html(data_row.isHidden ? "Yes" : "No");
            $('#eventCreatedDate').html(data_row.createdDate);
            $('#eventCreatedBy').html(data_row.createdBy);
            $('#eventUpdatedDate').html(data_row.updatedDate);
            $('#eventUpdatedBy').html(data_row.updatedBy);
        });
    });
});

function loadDataTable() {
    dataTable = $('#eventTable').DataTable({
        "ajax": {
            "url": "/Event/EventGet",
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
            { "data": "startDate", "width": "10%" },
            { "data": "endDate", "width": "10%" },
            {
                "data": "eventId",
                "render": function (data) {
                    return `<div class="text-center">
                                <span style="cursor:pointer">
                                    <img id="viewId" class='img-75' src="/img/view.png" alt="View Event Details" />
                                </span>
                                <a style="text-decoration:none;" href="/Event/Upsert?eventId=${data}">
                                    <img class='img-75' src="/img/edit.png" alt="Edit Event" />
                                </a>
                                <a style="text-decoration:none;" href=# onclick=Delete('/Event/Delete?eventId='+${data})>
                                    <img class='img-75' src="/img/delete.png" alt="Delete Event" />
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