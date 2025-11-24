$(document).ready(function () {
    loadPlans();
    loadExercisesForModal();
    loadClientsForModal();
    
    // Exercise search functionality
    $('#exerciseSearchInput').on('input', function() {
        const searchTerm = $(this).val().toLowerCase();
        $('.exercise-checkbox-item').each(function() {
            const exerciseName = $(this).find('label').text().toLowerCase();
            if (exerciseName.includes(searchTerm)) {
                $(this).show();
            } else {
                $(this).hide();
            }
        });
    });

    // Update selected exercises count
    $(document).on('change', '.exercise-checkbox', function() {
        updateSelectedExercisesCount();
    });

    // ✅ Load all exercises as checkboxes with modern design
    function loadExercisesForModal() {
        $.ajax({
            url: '/ExercisePlan/GetAllExercises',
            type: 'GET',
            success: function (data) {
                let html = '';
                data.forEach(ex => {
                    html += `
                        <div class="exercise-checkbox-item">
                            <div class="modern-checkbox">
                                <input class="form-check-input exercise-checkbox" type="checkbox" value="${ex.Id}" id="ex_${ex.Id}">
                                <label class="form-check-label modern-checkbox-label" for="ex_${ex.Id}">
                                    <div class="exercise-checkbox-content">
                                        <div class="exercise-name">${ex.Name}</div>
                                        <div class="exercise-details">
                                            <span class="exercise-muscle">${ex.MuscleName || 'Unknown'}</span>
                                            <span class="exercise-equipment">${ex.EquipmentName || 'None'}</span>
                                        </div>
                                    </div>
                                </label>
                            </div>
                        </div>
                    `;
                });
                $('#exerciseCheckboxes').html(html);
                updateSelectedExercisesCount();
                
                // Update total exercises count in overview
                $('#totalExercisesCount').text(data.length);
            },
            error: function () {
                $('#exerciseCheckboxes').html('<div class="text-danger text-center p-3">Failed to load exercises</div>');
                Swal.fire({
                    icon: 'error',
                    title: 'Loading Failed',
                    text: 'Failed to load exercises. Please try again.',
                    confirmButtonColor: '#ef4444'
                });
            }
        });
    }

    function updateSelectedExercisesCount() {
        const count = $('.exercise-checkbox:checked').length;
        $('#selectedExercisesCount').text(count);
    }

    // ✅ Load clients for the modal dropdown
    function loadClientsForModal() {
        $('#clientSelectorLoading').show();
        $('#clientSelector').prop('disabled', true);
        
        $.ajax({
            url: '/ExercisePlan/GetCoachClients',
            type: 'GET',
            success: function (data) {
                let options = '<option value="">Select a client...</option>';
                data.forEach(client => {
                    const statusBadge = client.IsActive ? '✓' : '⚠';
                    const statusText = client.IsActive ? 'Active' : 'Inactive';
                    options += `<option value="${client.Id}" ${!client.IsActive ? 'class="text-muted"' : ''}>
                        ${client.Name} ${statusBadge}
                    </option>`;
                });
                $('#clientSelector').html(options).prop('disabled', false);
                $('#clientSelectorLoading').hide();
            },
            error: function () {
                $('#clientSelector').html('<option value="">Failed to load clients</option>');
                $('#clientSelectorLoading').hide();
                Swal.fire({
                    icon: 'error',
                    title: 'Loading Failed',
                    text: 'Failed to load clients. Please try again.',
                    confirmButtonColor: '#ef4444'
                });
            }
        });
    }

    // ✅ When the "Create Plan" button is clicked
    $('#savePlanBtn').click(function () {
        // Clear any previous error states
        $('.form-control').removeClass('is-invalid');
        $('.invalid-feedback').remove();

        const selectedExercises = [];
        $('.exercise-checkbox:checked').each(function () {
            selectedExercises.push(parseInt($(this).val()));
        });

        const planData = {
            Name: $('input[name="Name"]').val().trim(),
            Notes: $('textarea[name="Notes"]').val().trim(),
            DurationWeeks: parseInt($('input[name="DurationWeeks"]').val()) || 0,
            ClientId: $('#clientSelector').val(),
            SelectedExerciseIds: selectedExercises
        };

        console.log("Data sent:", planData);

        // ✅ Enhanced validation before sending
        let isValid = true;
        let errorMessages = [];

        if (!planData.Name) {
            isValid = false;
            errorMessages.push("Plan name is required.");
            $('input[name="Name"]').addClass('is-invalid');
            $('input[name="Name"]').after('<div class="invalid-feedback">Plan name is required.</div>');
        }

        if (!planData.ClientId) {
            isValid = false;
            errorMessages.push("Please select a client for this plan.");
            $('#clientSelector').addClass('is-invalid');
            $('#clientSelector').after('<div class="invalid-feedback">Please select a client for this plan.</div>');
        }

        if (!planData.Notes) {
            isValid = false;
            errorMessages.push("Plan description is required.");
            $('textarea[name="Notes"]').addClass('is-invalid');
            $('textarea[name="Notes"]').after('<div class="invalid-feedback">Plan description is required.</div>');
        }

        if (planData.DurationWeeks <= 0) {
            isValid = false;
            errorMessages.push("Duration must be greater than 0 weeks.");
            $('input[name="DurationWeeks"]').addClass('is-invalid');
            $('input[name="DurationWeeks"]').after('<div class="invalid-feedback">Duration must be greater than 0 weeks.</div>');
        }

        if (selectedExercises.length === 0) {
            isValid = false;
            errorMessages.push("Please select at least one exercise.");
            $('.exercises-container').addClass('border-danger');
        }

        if (!isValid) {
            Swal.fire({
                icon: 'warning',
                title: 'Validation Error',
                html: errorMessages.join('<br>'),
                confirmButtonColor: '#f59e0b'
            });
            return;
        }

        // Show loading state
        const $saveBtn = $('#savePlanBtn');
        const originalText = $saveBtn.html();
        $saveBtn.prop('disabled', true).html('<i class="bi bi-hourglass-split me-2"></i>Creating Plan...');

        $.ajax({
            url: '/ExercisePlan/CreatePlan',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(planData),
            success: function (res) {
                Swal.fire({
                    icon: 'success',
                    title: 'Plan Created!',
                    text: res.message || 'Workout plan created successfully!',
                    confirmButtonColor: '#10b981',
                    timer: 2500
                }).then(() => {
                    // Reset form
                    $('#createPlanForm')[0].reset();
                    $('.exercise-checkbox').prop('checked', false);
                    $('#clientSelector').val('');
                    updateSelectedExercisesCount();
                    
                    // Close modal and reload plans
                    $('#createPlanModal').modal('hide');
                    loadPlans(); // 🔁 reload plans dynamically
                });
            },
            error: function (xhr) {
                let errMsg = "Error while saving plan.";
                try {
                    let errData = JSON.parse(xhr.responseText);
                    if (errData?.message) errMsg = errData.message;
                } catch { }
                Swal.fire({
                    icon: 'error',
                    title: 'Save Failed',
                    text: errMsg,
                    confirmButtonColor: '#ef4444'
                });
            },
            complete: function() {
                // Restore button state
                $saveBtn.prop('disabled', false).html(originalText);
                $('.exercises-container').removeClass('border-danger');
            }
        });
    });
});

// ✅ Function to load all plans dynamically with modern design
function loadPlans() {
    // Hide loading state
    $('#loadingState').hide();
    
    $.ajax({
        url: '/ExercisePlan/GetAllPlans',
        type: 'GET',
        success: function (plans) {
            let html = '';
            if (!plans || plans.length === 0) {
                html = `
                    <div class="empty-state">
                        <div class="empty-state-icon">
                            <i class="bi bi-calendar-x"></i>
                        </div>
                        <h5 class="empty-state-title">No Workout Plans Yet</h5>
                        <p class="empty-state-text">Create your first workout plan to get started</p>
                        <button class="btn btn-primary modern-btn" data-bs-toggle="modal" data-bs-target="#createPlanModal">
                            <i class="bi bi-plus-circle me-2"></i>Create Your First Plan
                        </button>
                    </div>
                `;
            } else {
                plans.forEach(p => {
                    // Split description by ,,, delimiter (supports both Arabic and English commas)
                    let descriptionCards = '';
                    
                    console.log('Plan Notes:', p.notes); // Debug log
                    
                    if (p.notes && p.notes.trim() !== '') {
                        // Try splitting with different delimiter patterns
                        let segments = [];
                        
                        // Check for various comma patterns (with or without spaces)
                        if (p.notes.includes(',,,')) {
                            // Split by ,,, and handle spaces
                            segments = p.notes.split(/\s*,,,\s*/);
                            console.log('Split by English commas (with spaces):', segments.length, 'segments');
                        } else if (p.notes.includes('،،،')) {
                            // Split by Arabic commas and handle spaces
                            segments = p.notes.split(/\s*،،،\s*/);
                            console.log('Split by Arabic commas (with spaces):', segments.length, 'segments');
                        } else {
                            // If no delimiter found, show as single card
                            segments = [p.notes];
                            console.log('No delimiter found, showing as single card');
                        }
                        
                        segments = segments.filter(segment => segment.trim() !== '');
                        
                        segments.forEach((segment, index) => {
                            descriptionCards += `
                                <div class="description-card mb-2">
                                    <div class="description-card-content">
                                        ${segment.trim()}
                                    </div>
                                </div>
                            `;
                        });
                    } else {
                        descriptionCards = '<div class="text-muted">No description available</div>';
                        console.log('No notes found for this plan');
                    }
                    
                    html += `
                        <div class="plan-card" data-plan-id="${p.id}">
                            <div class="plan-card-header">
                                <div class="plan-card-icon">
                                    <i class="bi bi-calendar-check"></i>
                                </div>
                                <div class="plan-card-title-section">
                                    <h5 class="plan-card-title">${p.name}</h5>
                                    <div class="plan-card-meta">
                                        <div class="plan-meta-item">
                                            <i class="bi bi-clock"></i>
                                            <span>${p.durationWeeks} weeks</span>
                                        </div>
                                        <div class="plan-meta-item">
                                            <i class="bi bi-people"></i>
                                            <span>${p.clientCount || 0} clients</span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            
                            <div class="plan-card-body">
                                <div class="plan-description">
                                    ${descriptionCards}
                                </div>
                                
                                <div class="plan-stats">
                                    <div class="plan-stat">
                                        <div class="stat-value">${p.exerciseCount || 0}</div>
                                        <div class="stat-label">Exercises</div>
                                    </div>
                                    <div class="plan-stat">
                                        <div class="stat-value">${p.durationWeeks}</div>
                                        <div class="stat-label">Weeks</div>
                                    </div>
                                    <div class="plan-stat">
                                        <div class="stat-value">${p.clientCount || 0}</div>
                                        <div class="stat-label">Clients</div>
                                    </div>
                                </div>
                            </div>

                            <div class="plan-card-actions">
                                <button class="plan-action-btn view" onclick="viewPlanDetails(${p.id})" title="View Details">
                                    <i class="bi bi-eye"></i>
                                </button>
                                <button class="plan-action-btn edit" onclick="editPlan(${p.id})" title="Edit Plan">
                                    <i class="bi bi-pencil"></i>
                                </button>
                                <button class="plan-action-btn assign" onclick="assignPlan(${p.id})" title="Assign to Client">
                                    <i class="bi bi-person-plus"></i>
                                </button>
                                <button class="plan-action-btn delete" onclick="deletePlan(${p.id})" title="Delete Plan">
                                    <i class="bi bi-trash"></i>
                                </button>
                            </div>
                        </div>
                    `;
                });
                
                // Update overview stats
                updateOverviewStats(plans);
            }
            $('#plansContainer').html(html);
        },
        error: function () {
            $('#plansContainer').html(`
                <div class="error-state">
                    <div class="error-state-icon">
                        <i class="bi bi-exclamation-triangle"></i>
                    </div>
                    <h5 class="error-state-title">Failed to Load Plans</h5>
                    <p class="error-state-text">There was an error loading your workout plans</p>
                    <button class="btn btn-outline-primary modern-btn" onclick="loadPlans()">
                        <i class="bi bi-arrow-clockwise me-2"></i>Try Again
                    </button>
                </div>
            `);
        }
    });
}

function updateOverviewStats(plans) {
    $('#totalPlansCount').text(plans.length);
    
    const totalClients = plans.reduce((sum, plan) => sum + (plan.clientCount || 0), 0);
    $('#activeClientsCount').text(totalClients);
    
    const avgDuration = plans.length > 0 ? 
        Math.round(plans.reduce((sum, plan) => sum + (plan.durationWeeks || 0), 0) / plans.length) : 0;
    $('#avgDurationWeeks').text(avgDuration);
}

// ✅ View Plan Details Function
function viewPlanDetails(id) {
    console.log('View plan details:', id);
    
    // Show the modal first
    $('#viewPlanModal').modal('show');
    
    // Load plan details
    $.ajax({
        url: '/ExercisePlan/GetPlanDetails',
        type: 'GET',
        data: { id: id },
        success: function(plan) {
            const exercisesList = plan.exercises.map(ex => `
                <div class="exercise-item">
                    <div class="exercise-info">
                        <h6 class="exercise-name">${ex.name}</h6>
                        <div class="exercise-meta">
                            <span class="badge bg-primary me-2">${ex.sets} sets</span>
                            <span class="badge bg-secondary me-2">${ex.reps} reps</span>
                            <span class="badge bg-info me-2">${ex.muscleName}</span>
                            <span class="badge bg-warning">${ex.equipmentName}</span>
                        </div>
                    </div>
                </div>
            `).join('');

            const content = `
                <div class="plan-details-view">
                    <div class="row g-4">
                        <div class="col-md-6">
                            <div class="detail-section">
                                <h6 class="section-title mb-3">
                                    <i class="bi bi-info-circle me-2"></i>Plan Information
                                </h6>
                                <div class="detail-item">
                                    <label class="detail-label">Plan Name:</label>
                                    <div class="detail-value">${plan.name}</div>
                                </div>
                                <div class="detail-item">
                                    <label class="detail-label">Duration:</label>
                                    <div class="detail-value">${plan.durationWeeks} weeks</div>
                                </div>
                                <div class="detail-item">
                                    <label class="detail-label">Created:</label>
                                    <div class="detail-value">${new Date(plan.createdDate).toLocaleDateString()}</div>
                                </div>
                                <div class="detail-item">
                                    <label class="detail-label">Description:</label>
                                    <div class="detail-value">${plan.notes || 'No description provided'}</div>
                                </div>
                            </div>
                        </div>
                        <div class="col-md-6">
                            <div class="detail-section">
                                <h6 class="section-title mb-3">
                                    <i class="bi bi-lightning me-2"></i>Exercises (${plan.exercises.length})
                                </h6>
                                <div class="exercises-list">
                                    ${exercisesList || '<p class="text-muted">No exercises assigned</p>'}
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            `;
            
            $('#viewPlanContent').html(content);
        },
        error: function(xhr) {
            let errMsg = "Failed to load plan details.";
            try {
                let errData = JSON.parse(xhr.responseText);
                if (errData?.message) errMsg = errData.message;
            } catch { }
            $('#viewPlanContent').html(`<div class="alert alert-danger">${errMsg}</div>`);
        }
    });
}

// ✅ Edit Plan Function
function editPlan(id) {
    console.log('Edit plan:', id);
    
    // Show the modal first
    $('#editPlanModal').modal('show');
    
    // Load clients for the edit modal
    loadClientsForEditModal();
    
    // Load exercises for the edit modal
    loadExercisesForEditModal();
    
    // Load plan details for editing
    $.ajax({
        url: '/ExercisePlan/GetPlanDetails',
        type: 'GET',
        data: { id: id },
        success: function(plan) {
            // Populate form fields
            $('#editPlanId').val(plan.id);
            $('#editPlanName').val(plan.name);
            $('#editPlanDuration').val(plan.durationWeeks);
            $('#editPlanNotes').val(plan.notes);
            $('#editClientSelector').val(plan.clientId);
            
            // Check the exercises that are part of this plan
            setTimeout(() => {
                plan.exercises.forEach(exercise => {
                    $(`#editExerciseCheckboxes input[value="${exercise.id}"]`).prop('checked', true);
                });
                updateEditSelectedExercisesCount();
            }, 500); // Wait for exercises to load
        },
        error: function(xhr) {
            let errMsg = "Failed to load plan for editing.";
            try {
                let errData = JSON.parse(xhr.responseText);
                if (errData?.message) errMsg = errData.message;
            } catch { }
            Swal.fire({
                icon: 'error',
                title: 'Loading Failed',
                text: errMsg,
                confirmButtonColor: '#ef4444'
            });
            $('#editPlanModal').modal('hide');
        }
    });
}

// ✅ Assign Plan Function
function assignPlan(id) {
    console.log('Assign plan:', id);
    
    // Show the modal first
    $('#assignPlanModal').modal('show');
    
    // Load clients for assignment
    loadClientsForAssignModal();
    
    // Load current plan details
    $.ajax({
        url: '/ExercisePlan/GetPlanDetails',
        type: 'GET',
        data: { id: id },
        success: function(plan) {
            $('#assignPlanId').val(plan.id);
            
            // Load current client info (you might need to implement this)
            $('#currentClientName').text('Loading current client...');
            $('#currentClientEmail').text('');
            
            // Load current client details
            loadCurrentClientInfo(plan.clientId);
        },
        error: function(xhr) {
            let errMsg = "Failed to load plan for assignment.";
            try {
                let errData = JSON.parse(xhr.responseText);
                if (errData?.message) errMsg = errData.message;
            } catch { }
            Swal.fire({
                icon: 'error',
                title: 'Loading Failed',
                text: errMsg,
                confirmButtonColor: '#ef4444'
            });
            $('#assignPlanModal').modal('hide');
        }
    });
}

// ✅ Helper Functions for Edit Modal
function loadClientsForEditModal() {
    $.ajax({
        url: '/ExercisePlan/GetCoachClients',
        type: 'GET',
        success: function (data) {
            let options = '<option value="">Select a client...</option>';
            data.forEach(client => {
                const statusBadge = client.IsActive ? '✓' : '⚠';
                options += `<option value="${client.Id}" ${!client.IsActive ? 'class="text-muted"' : ''}>
                    ${client.Name} ${statusBadge}
                </option>`;
            });
            $('#editClientSelector').html(options);
        },
        error: function () {
            $('#editClientSelector').html('<option value="">Failed to load clients</option>');
        }
    });
}

function loadExercisesForEditModal() {
    $.ajax({
        url: '/ExercisePlan/GetAllExercises',
        type: 'GET',
        success: function (data) {
            let html = '';
            data.forEach(ex => {
                html += `
                    <div class="exercise-checkbox-item">
                        <div class="modern-checkbox">
                            <input class="form-check-input edit-exercise-checkbox" type="checkbox" value="${ex.Id}" id="edit_ex_${ex.Id}">
                            <label class="form-check-label modern-checkbox-label" for="edit_ex_${ex.Id}">
                                <div class="exercise-checkbox-content">
                                    <div class="exercise-name">${ex.Name}</div>
                                    <div class="exercise-details">
                                        <span class="exercise-muscle">${ex.MuscleName || 'Unknown'}</span>
                                        <span class="exercise-equipment">${ex.EquipmentName || 'None'}</span>
                                    </div>
                                </div>
                            </label>
                        </div>
                    </div>
                `;
            });
            $('#editExerciseCheckboxes').html(html);
            
            // Add event listeners for edit modal
            $(document).on('change', '.edit-exercise-checkbox', function() {
                updateEditSelectedExercisesCount();
            });
            
            $('#editExerciseSearchInput').on('input', function() {
                const searchTerm = $(this).val().toLowerCase();
                $('#editExerciseCheckboxes .exercise-checkbox-item').each(function() {
                    const exerciseName = $(this).find('label').text().toLowerCase();
                    if (exerciseName.includes(searchTerm)) {
                        $(this).show();
                    } else {
                        $(this).hide();
                    }
                });
            });
        },
        error: function () {
            $('#editExerciseCheckboxes').html('<div class="text-danger text-center p-3">Failed to load exercises</div>');
        }
    });
}

function updateEditSelectedExercisesCount() {
    const count = $('.edit-exercise-checkbox:checked').length;
    $('#editSelectedExercisesCount').text(count);
}

// ✅ Helper Functions for Assign Modal
function loadClientsForAssignModal() {
    $.ajax({
        url: '/ExercisePlan/GetCoachClients',
        type: 'GET',
        success: function (data) {
            let options = '<option value="">Choose a different client...</option>';
            data.forEach(client => {
                const statusBadge = client.IsActive ? '✓' : '⚠';
                options += `<option value="${client.Id}" ${!client.IsActive ? 'class="text-muted"' : ''}>
                    ${client.Name} ${statusBadge}
                </option>`;
            });
            $('#assignClientSelector').html(options);
        },
        error: function () {
            $('#assignClientSelector').html('<option value="">Failed to load clients</option>');
        }
    });
}

function loadCurrentClientInfo(clientId) {
    // For now, we'll use a simple approach since we don't have a specific client details endpoint
    $.ajax({
        url: '/ExercisePlan/GetCoachClients',
        type: 'GET',
        success: function (data) {
            const currentClient = data.find(c => c.Id === clientId);
            if (currentClient) {
                $('#currentClientName').text(currentClient.Name);
                $('#currentClientEmail').text(currentClient.IsActive ? 'Active Client' : 'Inactive Client');
            } else {
                $('#currentClientName').text('Unknown Client');
                $('#currentClientEmail').text('Client not found');
            }
        },
        error: function () {
            $('#currentClientName').text('Error loading client');
            $('#currentClientEmail').text('');
        }
    });
}

// ✅ Event Handlers for Update and Assign Actions
$(document).ready(function() {
    // Update Plan Button Handler
    $('#updatePlanBtn').click(function () {
        // Clear any previous error states
        $('.form-control').removeClass('is-invalid');
        $('.invalid-feedback').remove();

        const selectedExercises = [];
        $('.edit-exercise-checkbox:checked').each(function () {
            selectedExercises.push(parseInt($(this).val()));
        });

        const planData = {
            Id: parseInt($('#editPlanId').val()),
            Name: $('#editPlanName').val().trim(),
            Notes: $('#editPlanNotes').val().trim(),
            DurationWeeks: parseInt($('#editPlanDuration').val()) || 0,
            ClientId: $('#editClientSelector').val(),
            SelectedExerciseIds: selectedExercises
        };

        // Validation
        let isValid = true;
        if (!planData.Name) {
            isValid = false;
            $('#editPlanName').addClass('is-invalid').after('<div class="invalid-feedback">Plan name is required.</div>');
        }
        if (!planData.ClientId) {
            isValid = false;
            $('#editClientSelector').addClass('is-invalid').after('<div class="invalid-feedback">Please select a client.</div>');
        }
        if (planData.DurationWeeks <= 0) {
            isValid = false;
            $('#editPlanDuration').addClass('is-invalid').after('<div class="invalid-feedback">Duration must be greater than 0.</div>');
        }
        if (selectedExercises.length === 0) {
            isValid = false;
            Swal.fire({
                icon: 'warning',
                title: 'Validation Error',
                text: 'Please select at least one exercise.',
                confirmButtonColor: '#f59e0b'
            });
            return;
        }

        if (!isValid) return;

        // Show loading state
        const $updateBtn = $('#updatePlanBtn');
        const originalText = $updateBtn.html();
        $updateBtn.prop('disabled', true).html('<i class="bi bi-hourglass-split me-2"></i>Updating...');

        $.ajax({
            url: '/ExercisePlan/UpdatePlan',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(planData),
            success: function (res) {
                Swal.fire({
                    icon: 'success',
                    title: 'Plan Updated!',
                    text: res.message || 'Workout plan updated successfully!',
                    confirmButtonColor: '#10b981',
                    timer: 2500
                }).then(() => {
                    $('#editPlanModal').modal('hide');
                    loadPlans();
                });
            },
            error: function (xhr) {
                let errMsg = "Error while updating plan.";
                try {
                    let errData = JSON.parse(xhr.responseText);
                    if (errData?.message) errMsg = errData.message;
                } catch { }
                Swal.fire({
                    icon: 'error',
                    title: 'Update Failed',
                    text: errMsg,
                    confirmButtonColor: '#ef4444'
                });
            },
            complete: function() {
                $updateBtn.prop('disabled', false).html(originalText);
            }
        });
    });

    // Assign Plan Button Handler
    $('#confirmAssignBtn').click(function () {
        const planId = parseInt($('#assignPlanId').val());
        const clientId = $('#assignClientSelector').val();

        if (!clientId) {
            Swal.fire({
                icon: 'warning',
                title: 'No Client Selected',
                text: 'Please select a client to assign this plan to.',
                confirmButtonColor: '#f59e0b'
            });
            return;
        }

        // Show loading state
        const $assignBtn = $('#confirmAssignBtn');
        const originalText = $assignBtn.html();
        $assignBtn.prop('disabled', true).html('<i class="bi bi-hourglass-split me-2"></i>Assigning...');

        $.ajax({
            url: '/ExercisePlan/AssignPlan',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ PlanId: planId, ClientId: clientId }),
            success: function (res) {
                Swal.fire({
                    icon: 'success',
                    title: 'Plan Assigned!',
                    text: res.message || 'Workout plan assigned successfully!',
                    confirmButtonColor: '#10b981',
                    timer: 2500
                }).then(() => {
                    $('#assignPlanModal').modal('hide');
                    loadPlans();
                });
            },
            error: function (xhr) {
                let errMsg = "Error while assigning plan.";
                try {
                    let errData = JSON.parse(xhr.responseText);
                    if (errData?.message) errMsg = errData.message;
                } catch { }
                Swal.fire({
                    icon: 'error',
                    title: 'Assignment Failed',
                    text: errMsg,
                    confirmButtonColor: '#ef4444'
                });
            },
            complete: function() {
                $assignBtn.prop('disabled', false).html(originalText);
            }
        });
    });
});

function deletePlan(id) {
    Swal.fire({
        title: 'Delete Workout Plan?',
        text: "This action cannot be undone. The workout plan will be permanently removed.",
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
                text: 'Please wait while we remove the workout plan',
                allowOutsideClick: false,
                allowEscapeKey: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });

            $.ajax({
                url: `/ExercisePlan/DeleteConfirmed/${id}`,
                method: 'POST',
                success: function (res) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Deleted Successfully!',
                        text: res.message || 'Workout plan has been removed.',
                        confirmButtonColor: '#10b981',
                        timer: 2000,
                        showConfirmButton: true
                    });
                    loadPlans();
                    
                    // Update statistics
                    $.ajax({
                        url: '/ExercisePlan/GetAllPlans',
                        type: 'GET',
                        success: function(plans) {
                            updateOverviewStats(plans);
                        }
                    });
                },
                error: function (xhr) {
                    let errMsg = 'Unable to delete the workout plan. Please try again.';
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
