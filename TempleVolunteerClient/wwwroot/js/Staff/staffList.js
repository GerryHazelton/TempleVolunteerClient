var dataTable;

$(staff).ready(function () {
    loadDataTable();
    debugger;
    $('#staffTable tbody').on('click', 'span', function () {
        var data_row = dataTable.row($(this).parents('tr')).data(); // here is the change
        $("#staffModal").modal('show');
        $('#staffModal').on('shown.bs.modal', function () {
            $('#staffFirstName').html(data_row.firstName);
            $('#staffMiddleName').html(data_row.middleName);
            $('#staffLastName').html(data_row.lastName);
            $('#staffNote').html(data_row.note);
            $('#staffIsActive').html(data_row.isActive ? "Yes" : "No");
            $('#staffCreatedDate').html(data_row.createdDate);
            $('#staffCreatedBy').html(data_row.createdBy);
            $('#staffUpdatedDate').html(data_row.updatedDate);
            $('#staffUpdatedBy').html(data_row.updatedBy);
        });
    });
});

function loadDataTable() {
    dataTable = $('#staffTable').DataTable({
        "ajax": {
            "url": "/Staff/StaffGet",
            "type": "GET",
            "datatype": "json",
            "serverSide": false,
            "error": function () {
                window.location.href = "/Account/StaffModalPopUp"; 
            }
        },
        "columns": [
            { "data": "name", "width": "10%" },
            { "data": "description", "width": "10%" },
            {
                "data": "staffId",
                "render": function (data) {
                    return `<div class="text-center">
                                <span style="cursor:pointer">
                                    <img id="viewId" class='img-75' src="/img/view.png" alt="View Staff Details" />
                                </span>
                                <a href="/Staff/StaffUpsert?staffId=${data}">
                                    <img class='img-75' src="/img/edit.png" alt="Edit Staff" />
                                </a>
                                <a href=# onclick=Delete('/Staff/StaffDelete?staffId='+${data})>
                                    <img class='img-75' src="/img/delete.png" alt="Delete Staff" />
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
                        toastr.success(data.staff);
                        dataTable.ajax.reload();
                    }
                    else {
                        toastr.error(data.staff);
                    }
                }
            });
        }
    });
}