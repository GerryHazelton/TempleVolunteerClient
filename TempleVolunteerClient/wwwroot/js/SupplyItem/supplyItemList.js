var dataTable;

$(supplyItem).ready(function () {
    loadDataTable();
    debugger;
    $('#supplyItemTable tbody').on('click', 'span', function () {
        var data_row = dataTable.row($(this).parents('tr')).data(); // here is the change
        $("#supplyItemModal").modal('show');
        $('#supplyItemModal').on('shown.bs.modal', function () {
            $('#supplyItemName').html(data_row.name);
            $('#supplyItemDescription').html(data_row.description);
            $('#supplyItemNote').html(data_row.note);
            $('#supplyItemIsActive').html(data_row.isActive ? "Yes" : "No");
            $('#supplyItemCreatedDate').html(data_row.createdDate);
            $('#supplyItemCreatedBy').html(data_row.createdBy);
            $('#supplyItemUpdatedDate').html(data_row.updatedDate);
            $('#supplyItemUpdatedBy').html(data_row.updatedBy);
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
                window.location.href = "/Account/SupplyItemModalPopUp"; 
            }
        },
        "columns": [
            { "data": "name", "width": "10%" },
            { "data": "description", "width": "10%" },
            {
                "data": "supplyItemId",
                "render": function (data) {
                    return `<div class="text-center">
                                <span style="cursor:pointer">
                                    <img id="viewId" class='img-75' src="/img/view.png" alt="View SupplyItem Details" />
                                </span>
                                <a href="/SupplyItem/SupplyItemUpsert?supplyItemId=${data}">
                                    <img class='img-75' src="/img/edit.png" alt="Edit SupplyItem" />
                                </a>
                                <a href=# onclick=Delete('/SupplyItem/SupplyItemDelete?supplyItemId='+${data})>
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
                        toastr.success(data.supplyItem);
                        dataTable.ajax.reload();
                    }
                    else {
                        toastr.error(data.supplyItem);
                    }
                }
            });
        }
    });
}