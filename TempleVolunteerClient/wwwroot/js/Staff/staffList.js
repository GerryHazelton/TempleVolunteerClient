var dataTable;

$(document).ready(function () {
    loadDataTable();

    $('#staffTable tbody').on('click', 'span', function () {
        var data_row = dataTable.row($(this).parents('tr')).data(); // here is the change
        $("#staffModal").modal('show');
        $('#staffModal').on('shown.bs.modal', function () {
            $('#staffImage').attr("src", "data:image/jpg;base64," + data_row.staffImage);
            $('#staffId').html(data_row.staffId);
            $('#staffName').html(data_row.name);
            $('#staffDescription').html(data_row.description);
            $('#staffNote').html(data_row.note);
            $('#staffQuantity').html(data_row.quantity);
            $('#staffBinNumber').html(data_row.binNumber);
            $('#staffIsActive').html(data_row.isActive ? "Yes" : "No");
            $('#staffIsHidden').html(data_row.isHidden ? "Yes" : "No");
            $('#staffCreatedDate').html(data_row.createdDate);
            $('#staffCreatedBy').html(data_row.createdBy);
            $('#staffUpdatedDate').html(data_row.updatedDate);
            $('#staffUpdatedBy').html(data_row.updatedBy);
            $('#staffFileName').html(data_row.staffFileName);
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
                window.location.href = "/Account/ErrorModalPopUp";
            }
        },
        "columns": [
            {
                "data": function (data) {
                    if (data.staffFileName != null && data.staffFileName != "undefined" && data.staffFileName.toLowerCase().trim().indexOf('jpg') > 0)
                        return "<span class='img-35'><img src=\"data:image/jpg;base64," + data.staffImage + "\" /></span>";

                    if (data.staffFileName != null && data.staffFileName != "undefined" && data.staffFileName.toLowerCase().trim().indexOf('png') > 0)
                        return "<span class='img-35'><img src=\"data:image/png;base64," + data.staffImage + "\" /></span>";

                    if (data.staffFileName != null && data.staffFileName != "undefined" && data.staffFileName.toLowerCase().trim().indexOf('gif') > 0)
                        return "<span class='img-35'><img src=\"data:image/gif;base64," + data.staffImage + "\" /></span>";

                    return "<span class='img-35'><img src=\"data:image/jpg;base64," + data.staffImage + "\" /></span>";
                },
                "width": "20%"
            },
            { "data": "firstName", "width": "10%" },
            { "data": "middleName", "width": "10%" },
            { "data": "lastName", "width": "10%" },
            { "data": "isActive", "width": "10%" },
            {
                "data": "staffId",
                "render": function (data) {
                    return `<div class="text-center">
                                <span style="cursor:pointer">
                                    <img id="viewId" class='img-75' src="/img/view.png" alt="View Staff Details" />
                                </span>
                                <a style="text-decoration:none;" href="/Staff/Upsert?staffId=${data}">
                                    <img class='img-75' src="/img/edit.png" alt="Edit Staff" />
                                </a>
                                <a style="text-decoration:none;" href=# onclick=Delete('/Staff/Delete?staffId='+${data})>
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