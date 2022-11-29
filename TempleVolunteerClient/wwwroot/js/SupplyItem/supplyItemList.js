var dataTable;

$(document).ready(function () {
    loadDataTable();

    $('#supplyItemTable tbody').on('click', 'span', function () {
        var data_row = dataTable.row($(this).parents('tr')).data(); // here is the change
        $("#supplyItemModal").modal('show');
        $('#supplyItemModal').on('shown.bs.modal', function () {
            $('#supplyItemImage').attr("src", "data:image/jpg;base64," + data_row.supplyItemImage);
            $('#supplyItemId').html(data_row.supplyItemId);
            $('#supplyItemName').html(data_row.name);
            $('#supplyItemDescription').html(data_row.description);
            $('#supplyItemNote').html(data_row.note);
            $('#supplyItemQuantity').html(data_row.quantity);
            $('#supplyItemBinNumber').html(data_row.binNumber);
            $('#supplyItemIsActive').html(data_row.isActive ? "Yes" : "No");
            $('#supplyItemIsHidden').html(data_row.isHidden ? "Yes" : "No");
            $('#supplyItemCreatedDate').html(data_row.createdDate);
            $('#supplyItemCreatedBy').html(data_row.createdBy);
            $('#supplyItemUpdatedDate').html(data_row.updatedDate);
            $('#supplyItemUpdatedBy').html(data_row.updatedBy);
            $('#supplyItemFileName').html(data_row.supplyItemFileName);
        });
    });
});

function loadDataTable() {
    dataTable = $('#supplyItemTable').DataTable({
        "ajax": {
            "url": "/SupplyItem/SupplyItemGet",
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
                    if (data.supplyItemFileName != null && data.supplyItemFileName != "undefined" && data.supplyItemFileName.toLowerCase().trim().indexOf('jpg') > 0)
                        return "<span class='img-35'><img src=\"data:image/jpg;base64," + data.supplyItemImage  + "\" /></span>";

                    if (data.supplyItemFileName != null && data.supplyItemFileName != "undefined" && data.supplyItemFileName.toLowerCase().trim().indexOf('png') > 0)
                        return "<span class='img-35'><img src=\"data:image/png;base64," + data.supplyItemImage + "\" /></span>";

                    if (data.supplyItemFileName != null && data.supplyItemFileName != "undefined" && data.supplyItemFileName.toLowerCase().trim().indexOf('gif') > 0)
                        return "<span class='img-35'><img src=\"data:image/gif;base64," + data.supplyItemImage + "\" /></span>";

                    return "<span class='img-35'><img src=\"data:image/jpg;base64," + data.supplyItemImage + "\" /></span>";
                },
                "width": "20%"
            },
            { "data": "name", "width": "10%" },
            { "data": "description", "width": "10%" },
            { "data": "quantity", "width": "10%" },
            {
                "data": "supplyItemId",
                "render": function (data) {
                    return `<div class="text-center">
                                <span style="cursor:pointer">
                                    <img id="viewId" class='img-75' src="/img/view.png" alt="View SupplyItem Details" />
                                </span>
                                <a style="text-decoration:none;" href="/SupplyItem/Upsert?supplyItemId=${data}">
                                    <img class='img-75' src="/img/edit.png" alt="Edit SupplyItem" />
                                </a>
                                <a style="text-decoration:none;" href=# onclick=Delete('/SupplyItem/Delete?supplyItemId='+${data})>
                                    <img class='img-75' src="/img/delete.png" alt="Delete SupplyItem" />
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