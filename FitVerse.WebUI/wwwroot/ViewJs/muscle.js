// ==========================
// Global Variables
// ==========================
let currentPage = 1;
let pageSize = 6;
let currentSearch = '';

// ==========================
// Document Ready
// ==========================
$(document).ready(function () {
    loadMusclePaged();
    loadAnatomyGroups(); // For Add Modal
    loadAnatomyFilter(); // For Filter Dropdown
    loadStats();

    // Search handler with debounce
    let searchTimeout;
    $('#searchMuscle').on('input', function () {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(function () {
            currentSearch = $('#searchMuscle').val().trim();
            currentPage = 1;
            loadMusclePaged();
        }, 300);
    });

    // Filter handler
    $('#anatomyFilter').on('change', function () {
        currentPage = 1;
        loadMusclePaged();
    });
});

// ==========================
// Load Muscles (Paged)
// ==========================
function loadMusclePaged() {
    let anatomyFilter = $('#anatomyFilter').val();

    const tbody = $('#Data');
    tbody.html(`
        <tr>
            <td colspan="4" class="text-center py-5">
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>
            </td>
        </tr>
    `);

    $.ajax({
        url: '/Muscle/GetPaged',
        method: 'GET',
        data: {
            page: currentPage,
            pageSize,
            search: currentSearch,
            anatomyId: anatomyFilter
        },
        success: function (response) {
            tbody.empty();
            const data = response.Data || [];

            if (!Array.isArray(data)) {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'Invalid data format returned from server.',
                    confirmButtonColor: '#6366f1'
                });
                console.error("Response:", response);
                return;
            }

            if (data.length === 0) {
                tbody.html(`
                    <tr>
                        <td colspan="4" class="empty-row">
                            <i class="bi bi-inbox" style="font-size: 3rem; color: #cbd5e1;"></i>
                            <div class="mt-2">
                                <strong>No muscles found</strong><br>
                                <small class="text-muted">
                                    ${currentSearch ? 'Try adjusting your search criteria' : 'Start by adding your first muscle'}
                                </small>
                            </div>
                        </td>
                    </tr>
                `);
            } else {
                data.forEach(item => {
                    tbody.append(`
                        <tr>
                            <td><strong>${item.Name}</strong></td>
                            <td>
                                <span class="badge-count">
                                    <i class="bi bi-person-arms-up me-1"></i>
                                    ${item.AnatomyName || 'Unknown'}
                                </span>
                            </td>
                            <td>
                                <span class="badge-count">
                                    <i class="bi bi-lightning me-1"></i>
                                    ${item.ExerciseCount || 0} exercises
                                </span>
                            </td>
                            <td class="text-end">
                                <button class="btn-table-edit me-2"
                                        data-id="${item.Id}"
                                        data-name="${encodeURIComponent(item.Name)}"
                                        data-description="${encodeURIComponent(item.Description || '')}"
                                        data-anatomy="${item.AnatomyId}"
                                        onclick="openEditFromBtn(this)">
                                    <i class="bi bi-pencil"></i> Edit
                                </button>
                                <button class="btn-table-delete"
                                        onclick="deleteMuscle(${item.Id})">
                                    <i class="bi bi-trash"></i> Delete
                                </button>
                            </td>
                        </tr>
                    `);
                });
            }

            renderPagination(response.CurrentPage || 1, response.TotalPages || 1);
        },
        error: function () {
            tbody.html(`
                <tr>
                    <td colspan="4" class="empty-row text-danger">
                        <i class="bi bi-exclamation-triangle" style="font-size: 3rem;"></i>
                        <div class="mt-2">
                            <strong>Failed to load muscles</strong><br>
                            <small>Please try refreshing the page</small>
                        </div>
                    </td>
                </tr>
            `);
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Failed to load muscles!',
                confirmButtonColor: '#6366f1'
            });
        }
    });
}

// ==========================
// Pagination
// ==========================
function renderPagination(currentPage, totalPages) {
    const pagination = $('.pagination-modern');
    pagination.empty();

    if (totalPages <= 1) return;

    const prevDisabled = currentPage === 1 ? 'disabled' : '';
    const nextDisabled = currentPage === totalPages ? 'disabled' : '';

    // Previous button
    pagination.append(`
        <li class="page-item ${prevDisabled}">
            <a class="page-link" href="#" onclick="changePage(${currentPage - 1}); return false;">
                <i class="bi bi-chevron-left"></i>
            </a>
        </li>
    `);

    // Page numbers
    for (let i = 1; i <= totalPages; i++) {
        const active = i === currentPage ? 'active' : '';
        pagination.append(`
            <li class="page-item ${active}">
                <a class="page-link" href="#" onclick="changePage(${i}); return false;">${i}</a>
            </li>
        `);
    }

    // Next button
    pagination.append(`
        <li class="page-item ${nextDisabled}">
            <a class="page-link" href="#" onclick="changePage(${currentPage + 1}); return false;">
                <i class="bi bi-chevron-right"></i>
            </a>
        </li>
    `);
}

function changePage(page) {
    if (page < 1) return;
    currentPage = page;
    loadMusclePaged();

    // Scroll to top of table
    $('html, body').animate({
        scrollTop: $('.card-modern').offset().top - 100
    }, 300);
}

// ==========================
// Load Anatomy Groups
// ==========================
function loadAnatomyGroups(selectId = '#AnatomyGroup', selectedId = null) {
    $.ajax({
        url: '/Muscle/GetAnatomyGroups',
        method: 'GET',
        success: function (response) {
            if (!response || !response.Data || !Array.isArray(response.Data)) {
                console.error("Invalid data format returned from server.", response);
                $(selectId).html('<option value="">Failed to load</option>');
                return;
            }

            const select = $(selectId);
            select.empty();
            select.append('<option value="">Select body part...</option>');

            response.Data.forEach(item => {
                const selected = item.Id === selectedId ? 'selected' : '';
                select.append(`<option value="${item.Id}" ${selected}>${item.Name}</option>`);
            });
        },
        error: function () {
            $(selectId).html('<option value="">Failed to load</option>');
        }
    });
}

// Load Anatomy for Filter Dropdown
function loadAnatomyFilter() {
    $.ajax({
        url: '/Muscle/GetAnatomyGroups',
        method: 'GET',
        success: function (response) {
            if (!response || !response.Data || !Array.isArray(response.Data)) {
                return;
            }

            const select = $('#anatomyFilter');
            select.empty();
            select.append('<option value="">All Body Parts</option>');

            response.Data.forEach(item => {
                select.append(`<option value="${item.Id}">${item.Name}</option>`);
            });
        }
    });
}

// ==========================
// Open Edit Modal
// ==========================
function openEditFromBtn(btn) {
    const id = $(btn).data('id');
    const name = decodeURIComponent($(btn).data('name'));
    const description = decodeURIComponent($(btn).data('description') || '');
    const anatomyId = $(btn).data('anatomy');

    $('#editMuscleId').val(id);
    $('#editName').val(name);
    $('#editDescription').val(description);

    loadAnatomyGroups('#editAnatomyGroup', anatomyId);
    $('#editMuscleModal').modal('show');
}

// ==========================
// CRUD Operations
// ==========================
function addMuscle() {
    const name = $('#Name').val().trim();
    const description = $('#Description').val().trim();
    const anatomyId = $('#AnatomyGroup').val();

    if (!name || !anatomyId) {
        Swal.fire({
            icon: 'warning',
            title: 'Missing Information',
            text: 'Please enter a name and select a body part.',
            confirmButtonColor: '#6366f1'
        });
        return;
    }

    Swal.fire({
        title: 'Saving...',
        text: 'Please wait',
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });

    $.post('/Muscle/Create',
        { Name: name, Description: description, AnatomyId: anatomyId },
        function (response) {
            Swal.close();
            const success = response.success ?? response.Success;
            const message = response.message ?? response.Message;

            if (success) {
                Swal.fire({
                    icon: 'success',
                    title: 'Success!',
                    text: message || 'Muscle added successfully!',
                    confirmButtonColor: '#6366f1',
                    timer: 2000
                });
                $('#addMuscleModal').modal('hide');
                $('#addMuscleForm')[0].reset();
                loadMusclePaged();
                loadStats();
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: message || 'Something went wrong!',
                    confirmButtonColor: '#6366f1'
                });
            }
        }
    ).fail(function () {
        Swal.fire({
            icon: 'error',
            title: 'Server Error',
            text: 'Failed to add muscle. Please try again.',
            confirmButtonColor: '#6366f1'
        });
    });
}

function updateMuscle() {
    const id = $('#editMuscleId').val();
    const name = $('#editName').val().trim();
    const description = $('#editDescription').val().trim();
    const anatomyId = $('#editAnatomyGroup').val();

    if (!name || !anatomyId) {
        Swal.fire({
            icon: 'warning',
            title: 'Missing Information',
            text: 'Name and Body Part are required!',
            confirmButtonColor: '#6366f1'
        });
        return;
    }

    Swal.fire({
        title: 'Updating...',
        text: 'Please wait',
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });

    $.post('/Muscle/Update',
        { Id: id, Name: name, Description: description, AnatomyId: anatomyId },
        function (response) {
            Swal.close();
            const success = response.Success ?? response.success;
            const message = response.Message ?? response.message;

            if (success) {
                Swal.fire({
                    icon: 'success',
                    title: 'Updated!',
                    text: message || 'Updated successfully!',
                    confirmButtonColor: '#6366f1',
                    timer: 2000
                });
                $('#editMuscleModal').modal('hide');
                loadMusclePaged();
                loadStats();
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: message || 'Something went wrong!',
                    confirmButtonColor: '#6366f1'
                });
            }
        }
    ).fail(function () {
        Swal.fire({
            icon: 'error',
            title: 'Server Error',
            text: 'Failed to update muscle. Please try again.',
            confirmButtonColor: '#6366f1'
        });
    });
}

function deleteMuscle(id) {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#ef4444',
        cancelButtonColor: '#6b7280',
        confirmButtonText: 'Yes, delete it!',
        cancelButtonText: 'Cancel'
    }).then(willDelete => {
        if (willDelete.isConfirmed) {
            Swal.fire({
                title: 'Deleting...',
                text: 'Please wait',
                allowOutsideClick: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });

            $.post('/Muscle/Delete', { id }, function (response) {
                Swal.close();
                if (response.success) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Deleted!',
                        text: response.message,
                        confirmButtonColor: '#6366f1',
                        timer: 2000
                    });
                    loadMusclePaged();
                    loadStats();
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: response.message,
                        confirmButtonColor: '#6366f1'
                    });
                }
            }).fail(function () {
                Swal.fire({
                    icon: 'error',
                    title: 'Server Error',
                    text: 'Failed to delete muscle. Please try again.',
                    confirmButtonColor: '#6366f1'
                });
            });
        }
    });
}

// ==========================
// Load Statistics
// ==========================
function loadStats() {
    $.ajax({
        url: '/Muscle/GetStats',
        method: 'GET',
        success: function (response) {
            $('#totalMuscles').text(response.TotalMuscles || 0);
            $('#totalBodyParts').text(response.TotalAnatomyGroups || 0);
            $('#totalExercises').text(response.TotalExercises || 0);
        },
        error: function () {
            console.error("Failed to load stats");
            // Set to 0 if fails
            $('#totalMuscles').text('0');
            $('#totalBodyParts').text('0');
            $('#totalExercises').text('0');
        }
    });
}

// ==========================
// Modal Cleanup
// ==========================
$('#addMuscleModal').on('hidden.bs.modal', function () {
    $('#addMuscleForm')[0].reset();
});

$('#editMuscleModal').on('hidden.bs.modal', function () {
    $('#editMuscleForm')[0].reset();
});