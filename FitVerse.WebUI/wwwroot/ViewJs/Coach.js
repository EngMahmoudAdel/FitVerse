
// Form submission removed - Admin can only view and delete coaches
// Coaches are managed through their own profile pages



$(document).ready(function () {
    LoadCoaches();
});

function LoadCoaches() {
    $.ajax({
        url: '/Coach/GetAllCoaches',
        method: 'GET',
        success: function (res) {
            if (res.success) {
                let coaches = res.data;
                let coachTableBody = $("#Data");
                coachTableBody.empty();

                if (coaches.length === 0) {
                    coachTableBody.append(`
                        <tr>
                            <td colspan="5" class="text-center py-5">
                                <div class="empty-state">
                                    <i class="bi bi-person-badge"></i>
                                    <h6>No coaches found</h6>
                                    <p class="mb-0">Start by adding your first coach to the system.</p>
                                </div>
                            </td>
                        </tr>
                    `);
                } else {
                    coaches.forEach(coach => {
                        // Status badge
                        let statusBadge = coach.IsActive 
                            ? '<span class="badge bg-success">✅ Active</span>'
                            : '<span class="badge bg-danger">❌ Inactive</span>';

                        // Avatar URL (supports legacy /img/... paths and new /profile-image/{fileName})
                        let avatarUrl;
                        if (coach.ImagePath && coach.ImagePath.trim() !== '') {
                            if (coach.ImagePath.startsWith('/')) {
                                avatarUrl = coach.ImagePath;
                            } else {
                                avatarUrl = `/profile-image/${coach.ImagePath}`;
                            }
                        } else {
                            avatarUrl = `https://ui-avatars.com/api/?name=${encodeURIComponent(coach.Name)}&background=6366f1&color=fff`;
                        }

                        let row = `
                            <tr>
                                <td>${coach.Name || 'Unknown'}</td>
                                <td>${coach.Title || 'Coach'}</td>
                                <td>${coach.About || 'No description'}</td>
                                <td class="text-center">
                                    <img src="${avatarUrl}" alt="${coach.Name}" 
                                         style="width: 50px; height: 50px; border-radius: 50%; object-fit: cover;">
                                </td>
                                <td class="text-center">${statusBadge}</td>
                                <td class="text-center">
                                    <button type="button" onclick="DeleteCoach('${coach.Id}')" 
                                            class="btn btn-sm btn-outline-danger" title="Delete Coach">
                                        <i class="bi bi-trash"></i>
                                    </button>
                                </td>
                            </tr>`;
                        coachTableBody.append(row);
                    });
                }

                // Update pagination info
                $('#paginationInfo').text(`Showing ${coaches.length} of ${coaches.length} coaches`);
                
            } else {
                Swal.fire("Error", res.message, "error");
            }
        },
        error: function () {
            Swal.fire("Warning", "An error occurred while fetching coaches.", "error");
        }
    });
}

// Edit function removed - Admin can only view and delete coaches

function DeleteCoach(id) {
    Swal.fire({
        title: "Are you sure?",
        text: "This will delete the coach and all related data (plans, feedbacks, chats, etc.). This action cannot be undone!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Yes, delete it!',
        cancelButtonText: 'Cancel',
        showLoaderOnConfirm: true,
        preConfirm: () => {
            return $.ajax({
                url: '/Coach/DeleteCoach',
                method: 'POST',
                data: { Id: id },
                dataType: 'json'
            }).then(response => {
                if (!response.success) {
                    throw new Error(response.message);
                }
                return response;
            }).catch(error => {
                Swal.showValidationMessage(
                    `Request failed: ${error.message || error.responseJSON?.message || 'Unknown error'}`
                );
            });
        },
        allowOutsideClick: () => !Swal.isLoading()
    }).then((result) => {
        if (result.isConfirmed && result.value) {
            Swal.fire({
                icon: 'success',
                title: 'Deleted!',
                text: result.value.message || 'Coach has been deleted successfully.',
                timer: 2000,
                showConfirmButton: false
            });
            LoadCoaches();
        }
    });
}
let currentPage = 1;
let pageSize = 10;
let currentSearch = "";

function loadEquipments(page = 1, search = "") {
    currentPage = page;
    currentSearch = search;

    $.ajax({
        url: '/Coach/GetPaged',
        method: 'GET',
        data: { page: page, pageSize: pageSize, search: search },
        success: function (res) {
            let tbody = $('#Data');
            tbody.empty();

            if (!res.data || res.data.length === 0) {
                tbody.append('<tr><td colspan="6">No results found.</td></tr>');
            } else {
                res.data.forEach(item => {
                    let status = item.IsActive ? "✅ Active" : "❌ Inactive";

                    let imageUrl;
                    if (item.ImagePath && item.ImagePath.trim() !== '') {
                        if (item.ImagePath.startsWith('/')) {
                            imageUrl = item.ImagePath;
                        } else {
                            imageUrl = `/profile-image/${item.ImagePath}`;
                        }
                    } else {
                        imageUrl = `https://ui-avatars.com/api/?name=${encodeURIComponent(item.Name)}&background=6366f1&color=fff`;
                    }

                    let row = `
                        <tr>
                            <td>${item.Name}</td>
                            <td>${item.Title}</td>
                            <td>${item.About}</td>
                            <td><img src="${imageUrl}" alt="${item.Name}" width="80"/></td>
                            <td>${status}</td>
                            <td class="actions">
                                <button type="button" onclick="getById('${item.Id}')" class="btn-icon" title="Edit">
                                    <i class="fas fa-edit"></i>
                                </button>
                                <button type="button" onclick="DeleteCoach('${item.Id}')" class="btn-icon text-danger" title="Delete">
                                    <i class="fas fa-trash-alt"></i>
                                </button>
                            </td>
                        </tr>`;
                    tbody.append(row);
                });
            }

            renderPagination(res.totalPages);
        },
        error: function () {
            alert('Error loading equipments.');
        }
    });
}

function renderPagination(totalPages) {
    let pagination = $('#pagination');
    pagination.empty();

    for (let i = 1; i <= totalPages; i++) {
        let active = (i === currentPage) ? 'active' : '';
        pagination.append(`
            <li class="page-item ${active}">
                <a class="page-link" href="#" onclick="loadEquipments(${i}, '${currentSearch}')">${i}</a>
            </li>
        `);
    }
}

$('#searchInput').on('keyup', function () {
    let searchTerm = $(this).val().trim();
    loadEquipments(1, searchTerm); 
});

$(document).ready(function () {
    loadEquipments();
});
