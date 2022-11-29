var dataTable;

$(document).ready(function () {
    loadDataTable();

    $('#credentialTable tbody').on('click', 'span', function () {
        var data_row = dataTable.row($(this).parents('tr')).data(); // here is the change
        $("#credentialModal").modal('show');
        $('#credentialModal').on('shown.bs.modal', function () {
            $('#credentialId').html(data_row.credentialId);
            $('#credentialName').html(data_row.name);
            $('#credentialDescription').html(data_row.description);
            $('#credentialNote').html(data_row.note);
            $('#credentialIsActive').html(data_row.isActive ? "Yes" : "No");
            $('#credentialIsHidden').html(data_row.isHidden ? "Yes" : "No");
            $('#credentialCreatedDate').html(data_row.createdDate);
            $('#credentialCreatedBy').html(data_row.createdBy);
            $('#credentialUpdatedDate').html(data_row.updatedDate);
            $('#credentialUpdatedBy').html(data_row.updatedBy);
        });
    });
});

function loadDataTable() {
    dataTable = $('#credentialTable').DataTable({
        "ajax": {
            "url": "/Credential/CredentialGet",
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
                "data": "credentialId",
                "render": function (data) {
                    return `<div class="text-center">
                                <span style="cursor:pointer">
                                    <img id="viewId" class='img-75' src="/img/view.png" alt="View Credential Details" />
                                </span>
                                <a style="text-decoration:none;" href="/Credential/Upsert?credentialId=${data}">
                                    <img class='img-75' src="/img/edit.png" alt="Edit Credential" />
                                </a>
                                <a style="text-decoration:none;" href=# onclick=Delete('/Credential/Delete?credentialId='+${data})>
                                    <img class='img-75' src="/img/delete.png" alt="Delete Credential" />
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