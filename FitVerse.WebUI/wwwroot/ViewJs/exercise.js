let currentPage = 1;
let pageSize = 6;
let currentSearch = "";

$(document).ready(function () {
    loadMuscles();
    loadEquipments();
    loadExercisesPaged();

    // ✅ live search
    $('#searchInput').on('input', function () {
        currentSearch = $(this).val().trim();
        currentPage = 1;
        loadExercisesPaged();
    });

    // ✅ form submit
    $('#exerciseForm').on('submit', function (e) {
        e.preventDefault();
        let id = $('#exerciseId').val();
        if (id) updateExercise();
        else addExercise();

        e.target.reset();
    });

    // ✅ reset button
    $('#resetBtn').click(clearForm);
});

// Search feature
$('#searchExercise').on('input', function () {
    let value = $(this).val().toLowerCase();
    $('#exerciseTable tbody tr').filter(function () {
        $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1);
    });
});

// ======================= LOAD DATA =========================
function loadMuscles() {
    $.ajax({
        url: '/Muscle/GetAll',
        method: 'GET',
        success: function (response) {
            let dropdown = $('#muscleId');
            dropdown.empty().append('<option value="" disabled selected>Select muscle</option>');
            
            console.log('Muscles API response:', response); // Debug log
            
            let muscles = [];
            
            // Handle different response formats
            if (response && response.data && Array.isArray(response.data)) {
                muscles = response.data;
            } else if (response && Array.isArray(response)) {
                muscles = response;
            } else if (response && response.result && Array.isArray(response.result)) {
                muscles = response.result;
            } else if (response && response.items && Array.isArray(response.items)) {
                muscles = response.items;
            } else if (response && typeof response === 'object') {
                // If it's an object, try to find array properties
                const possibleArrays = Object.values(response).filter(val => Array.isArray(val));
                if (possibleArrays.length > 0) {
                    muscles = possibleArrays[0];
                } else {
                    console.error('No array found in response object:', response);
                    return;
                }
            } else {
                console.error('Invalid response format for muscles:', response);
                return;
            }
            
            if (muscles.length > 0) {
                muscles.forEach(m => {
                    // Handle different property name formats
                    const id = m.Id || m.id || m.ID;
                    const name = m.Name || m.name || m.title || m.Title;
                    if (id && name) {
                        dropdown.append(`<option value="${id}">${name}</option>`);
                    }
                });
            }
        },
        error: function () {
            Swal.fire({
                icon: 'error',
                title: 'Loading Failed',
                text: 'Failed to load muscles!',
                confirmButtonColor: '#ef4444'
            });
        }
    });
}


function loadEquipments() {
    $.ajax({
        url: '/Equipment/GetAll',
        method: 'GET',
        success: function (response) {
            let dropdown = $('#equipmentId');
            dropdown.empty().append('<option value="" disabled selected>Select equipment</option>');
            
            console.log('Equipment API response:', response); // Debug log
            
            let equipment = [];
            
            // Handle different response formats
            if (response && response.data && Array.isArray(response.data)) {
                equipment = response.data;
            } else if (response && Array.isArray(response)) {
                equipment = response;
            } else if (response && response.result && Array.isArray(response.result)) {
                equipment = response.result;
            } else if (response && response.items && Array.isArray(response.items)) {
                equipment = response.items;
            } else if (response && typeof response === 'object') {
                // If it's an object, try to find array properties
                const possibleArrays = Object.values(response).filter(val => Array.isArray(val));
                if (possibleArrays.length > 0) {
                    equipment = possibleArrays[0];
                } else {
                    console.error('No array found in response object:', response);
                    return;
                }
            } else {
                console.error('Invalid response format for equipment:', response);
                return;
            }
            
            if (equipment.length > 0) {
                equipment.forEach(eq => {
                    // Handle different property name formats
                    const id = eq.Id || eq.id || eq.ID;
                    const name = eq.Name || eq.name || eq.title || eq.Title;
                    if (id && name) {
                        dropdown.append(`<option value="${id}">${name}</option>`);
                    }
                });
            }
        },
        error: function () {
            Swal.fire({
                icon: 'error',
                title: 'Loading Failed',
                text: 'Failed to load equipment!',
                confirmButtonColor: '#ef4444'
            });
        }
    });
}


// ======================= EXERCISES (MAIN) =========================
function loadExercisesPaged() {
    $.ajax({
        url: '/Exercise/GetPaged',
        method: 'GET',
        data: { page: currentPage, pageSize: pageSize, search: currentSearch },
        success: function (response) {
            let container = $('#exerciseContainer');
            container.empty();

            // Check if response has data property and it's an array
            let exercises = [];
            if (response && response.data && Array.isArray(response.data)) {
                exercises = response.data;
            } else if (response && Array.isArray(response)) {
                exercises = response;
            } else {
                console.error('Invalid response format for exercises:', response);
                container.html('<p class="text-center text-muted mt-3">Error loading exercises.</p>');
                $('#exercisePagination').empty();
                return;
            }

            if (exercises.length === 0) {
                container.html('<p class="text-center text-muted mt-3">No exercises found.</p>');
                $('#exercisePagination').empty();
                return;
            }

            exercises.forEach(item => {
                let imageUrl = item.ImageUrl && item.ImageUrl.trim() !== "" ? item.ImageUrl : '/images/default-exercise.jpg';
                let muscleName = item.MuscleName || "Unknown";
                let equipmentName = item.EquipmentName || "None";
                let description = item.Description || "No description available.";
                let videoLink = item.VideoLink && item.VideoLink.trim() !== "" ? item.VideoLink : null;

                // Truncate description for better display
                let truncatedDescription = description.length > 120 ? description.substring(0, 120) + '...' : description;

                container.append(`
                    <div class="exercise-card" data-exercise-id="${item.Id}">
                        <div class="exercise-card-header">
                            <div class="exercise-card-image">
                                <img src="${imageUrl}" alt="${item.Name}" onerror="this.src='/images/default-exercise.jpg'">
                                <div class="exercise-card-overlay">
                                    <div class="exercise-actions-quick">
                                        ${videoLink ? `<a href="${videoLink}" target="_blank" class="quick-action-btn video-btn" title="Watch Video">
                                            <i class="bi bi-play-circle-fill"></i>
                                        </a>` : ''}
                                    </div>
                                </div>
                            </div>
                            <div class="exercise-card-title-section">
                                <h5 class="exercise-card-title">${item.Name}</h5>
                                <div class="exercise-card-meta">
                                    <div class="exercise-meta-item">
                                        <i class="bi bi-person-arms-up"></i>
                                        <span>${muscleName}</span>
                                    </div>
                                    <div class="exercise-meta-item">
                                        <i class="bi bi-tools"></i>
                                        <span>${equipmentName}</span>
                                    </div>
                                </div>
                            </div>
                        </div>
                        
                        <div class="exercise-card-body">
                            <div class="exercise-description">
                                ${truncatedDescription}
                            </div>
                        </div>

                        <div class="exercise-card-actions">
                            <button class="exercise-action-btn edit" onclick="getExerciseById(${item.Id})" title="Edit Exercise">
                                <i class="bi bi-pencil"></i>
                            </button>
                            <button class="exercise-action-btn view" onclick="viewExerciseDetails(${item.Id})" title="View Details">
                                <i class="bi bi-eye"></i>
                            </button>
                            ${videoLink ? `<a href="${videoLink}" target="_blank" class="exercise-action-btn video" title="Watch Video">
                                <i class="bi bi-play-circle"></i>
                            </a>` : ''}
                            <button class="exercise-action-btn delete" onclick="deleteExercise(${item.Id})" title="Delete Exercise">
                                <i class="bi bi-trash"></i>
                            </button>
                        </div>
                    </div>
                `);
            });

            renderPagination(response.currentPage, response.totalPages);
            
            // Update exercise count
            $('#exerciseCount').text(response.totalCount || exercises.length);
        },
        error: function (xhr) {
            console.error("Failed to load exercises.", xhr.responseText);
        }
    });
}



// ======================= PAGINATION =========================
// ==========================
// Pagination
// ==========================
function renderPagination(currentPage, totalPages) {
    const pagination = $('#exercisePagination');
    pagination.empty();

    const prevDisabled = currentPage === 1 ? 'disabled' : '';
    const nextDisabled = currentPage === totalPages ? 'disabled' : '';

    pagination.append(`<li class="page-item ${prevDisabled}"><a class="page-link" href="#" onclick="changePage(${currentPage - 1})">Previous</a></li>`);

    for (let i = 1; i <= totalPages; i++) {
        const active = i === currentPage ? 'active' : '';
        pagination.append(`<li class="page-item ${active}"><a class="page-link" href="#" onclick="changePage(${i})">${i}</a></li>`);
    }

    pagination.append(`<li class="page-item ${nextDisabled}"><a class="page-link" href="#" onclick="changePage(${currentPage + 1})">Next</a></li>`);
}

function changePage(page) {
    if (page < 1) return;
    currentPage = page;
    loadExercisesPaged();
}

// ======================= CRUD =========================
function addExercise() {
    let exercise = {
        Name: $('#Name').val(),
        MuscleId: $('#muscleId').val(),
        EquipmentId: $('#equipmentId').val(),
        VideoLink: $('#videoLink').val(),
        Description: $('#description').val()
    };

    $.ajax({
        url: '/Exercise/Create',
        method: 'POST',
        data: exercise,
        success: function (response) {
            if (response.success) {
                Swal.fire({
                    icon: 'success',
                    title: 'Exercise Added!',
                    text: response.message || 'Exercise added successfully!',
                    confirmButtonColor: '#10b981',
                    timer: 2500
                }).then(() => {
                    $('#addExerciseModal').modal('hide');
                    $('#exerciseForm')[0].reset();
                    loadExercisesPaged(); // يعيد تحميل التمارين
                });
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: response.message,
                    confirmButtonColor: '#ef4444'
                });
            }
        },
        error: function () {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Failed to add exercise!',
                confirmButtonColor: '#ef4444'
            });
        }
    });
}

function getExerciseById(Id) {
    $.ajax({
        url: '/Exercise/GetById?id=' + Id,
        method: 'GET',
        success: function (response) {
            if (response.success) {
                let data = response.data;
                $('#exerciseId').val(data.Id);
                $('#Name').val(data.Name);
                $('#muscleId').val(data.MuscleId);
                $('#equipmentId').val(data.EquipmentId);
                $('#videoLink').val(data.VideoLink);
                $('#description').val(data.Description);
                $('#saveBtn').html('<i class="fas fa-save"></i> Update Exercise');
                $('#addExerciseModal').modal('show');
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: response.message,
                    confirmButtonColor: '#ef4444'
                });
            }
        },
        error: function () {
            Swal.fire({
                icon: 'error',
                title: 'Loading Failed',
                text: 'Failed to fetch exercise!',
                confirmButtonColor: '#ef4444'
            });
        }
    });
}


function updateExercise() {
    let exercise = {
        Id: $('#exerciseId').val(),
        Name: $('#Name').val(),
        MuscleId: $('#muscleId').val(),
        EquipmentId: $('#equipmentId').val(),
        VideoLink: $('#videoLink').val(),
        Description: $('#description').val()
    };

    $.ajax({
        url: '/Exercise/Update',
        method: 'POST',
        data: exercise,
        success: function (response) {
            if (response.success) {
                Swal.fire({
                    icon: 'success',
                    title: 'Exercise Updated!',
                    text: response.message || 'Exercise updated successfully!',
                    confirmButtonColor: '#10b981',
                    timer: 2500
                });
                $('#addExerciseModal').modal('hide');
                clearForm();
                loadExercisesPaged();
                $('#saveBtn').html('<i class="fas fa-save"></i> Save Exercise');
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Update Failed',
                    text: response.message,
                    confirmButtonColor: '#ef4444'
                });
            }
        },
        error: function () {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Failed to update exercise!',
                confirmButtonColor: '#ef4444'
            });
        }
    });
}


function deleteExercise(Id) {
    Swal.fire({
        title: 'Delete Exercise?',
        text: "This action cannot be undone. The exercise will be permanently removed.",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#ef4444',
        cancelButtonColor: '#6b7280',
        confirmButtonText: '<i class="bi bi-trash me-2"></i>Yes, Delete It',
        cancelButtonText: '<i class="bi bi-x-circle me-2"></i>Cancel',
        reverseButtons: true,
        focusCancel: true
    }).then((result) => {
        if (result.isConfirmed) {
            // Show loading
            Swal.fire({
                title: 'Deleting...',
                text: 'Please wait while we remove the exercise',
                allowOutsideClick: false,
                allowEscapeKey: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });

            $.ajax({
                url: '/Exercise/Delete?id=' + Id,
                method: 'POST',
                success: function (response) {
                    if (response.success) {
                        Swal.fire({
                            icon: 'success',
                            title: 'Deleted Successfully!',
                            text: response.message || 'Exercise has been removed.',
                            confirmButtonColor: '#10b981',
                            timer: 2000,
                            showConfirmButton: true
                        });
                        loadExercisesPaged();
                        
                        // Remove the card with animation
                        $(`.exercise-card[data-exercise-id="${Id}"]`).fadeOut(400, function() {
                            $(this).remove();
                        });
                    } else {
                        Swal.fire({
                            icon: 'error',
                            title: 'Deletion Failed',
                            text: response.message || 'Unable to delete the exercise.',
                            confirmButtonColor: '#ef4444'
                        });
                    }
                },
                error: function (xhr) {
                    let errMsg = 'Unable to delete the exercise. Please try again.';
                    try {
                        let errData = JSON.parse(xhr.responseText);
                        if (errData?.message) errMsg = errData.message;
                    } catch { }
                    Swal.fire({
                        icon: 'error',
                        title: 'Deletion Failed',
                        text: errMsg,
                        confirmButtonColor: '#ef4444'
                    });
                }
            });
        }
    });
}


function clearForm() {
    $('#exerciseId').val('');
    $('#Name').val('');
    $('#muscleId').val('');
    $('#equipmentId').val('');
    $('#videoLink').val('');
    $('#description').val('');
}

function viewExerciseDetails(id) {
    // This function can be used to show exercise details in a modal or expand the card
    // For now, it will trigger the edit function to maintain existing functionality
    getExerciseById(id);
}

$('#viewAllClientsBtn').on('click', function () {
    $('#allClientsContainer').html('<div class="text-center text-muted">Loading...</div>');

    $.ajax({
        url: '/Coach/GetAllClients',
        method: 'GET',
        success: function (response) {
            let clients = response.data;
            let html = '';

            if (clients.length > 0) {
                clients.forEach(client => {
                    html += `
                        <div class="d-flex align-items-center justify-content-between mb-3 pb-3 border-bottom">
                            <div class="d-flex align-items-center gap-3">
                                <div>
                                    <div class="fw-semibold">${client.Name}</div>
                                    <div class="text-muted small">Last payment: ${client.LastPaymentAgo}</div>
                                </div>
                            </div>
                            <div>
                                <span class="badge-custom ${client.IsActive ? 'badge-success' : 'badge-warning'}">
                                    ${client.IsActive ? 'Active' : 'Inactive'}
                                </span>
                            </div>
                        </div>
                    `;
                });
            } else {
                html = '<div class="text-center text-muted">No clients found.</div>';
            }

            $('#allClientsContainer').html(html);
        },
        error: function () {
            $('#allClientsContainer').html('<div class="text-center text-danger">Failed to load clients.</div>');
        }
    });

    $('#viewAllClientsModal').modal('show');
});


