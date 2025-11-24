$(document).ready(function () {

    // 🟢 Load all diet plans on page load
    loadDietPlans();
    loadStatistics();
    
    // Enhanced UI interactions
    initializeEnhancedFeatures();
   

    //// 🟢 دالة تحميل كل الخطط
    //function loadDietPlans() {
    //    $.ajax({
    //        url: '/DietPlan/GetAll',
    //        method: 'GET',
    //        success: function (response) {
    //            $('#dietPlansContainer').empty();

    //            response.data.forEach(function (plan) {
    //                $('#dietPlansContainer').append(`
    //                    <div class="col-lg-4 col-md-6 mb-4">
    //                        <div class="card-custom h-100 shadow-sm">
    //                            <div class="card-body-custom">
    //                                <div class="d-flex justify-content-between align-items-start mb-3">
    //                                    <div>
    //                                        <h5 class="fw-bold mb-1">${plan.name}</h5>
    //                                        <small class="text-muted">${plan.totalCal} calories/day</small>
    //                                    </div>
    //                                    <div class="dropdown">
    //                                        <button class="btn btn-sm btn-outline-custom" data-bs-toggle="dropdown">
    //                                            <i class="bi bi-three-dots-vertical"></i>
    //                                        </button>
    //                                        <ul class="dropdown-menu">
    //                                            <li><a class="dropdown-item view-plan" data-id="${plan.id}" href="#"><i class="bi bi-eye me-2"></i>View Details</a></li>
    //                                            <li><a class="dropdown-item edit-plan" data-id="${plan.id}" href="#"><i class="bi bi-pencil me-2"></i>Edit</a></li>
    //                                            <li><a class="dropdown-item delete-plan text-danger" data-id="${plan.id}" href="#"><i class="bi bi-trash me-2"></i>Delete</a></li>
    //                                        </ul>
    //                                    </div>
    //                                </div>

    //                                <div class="mb-3">
    //                                    <div class="d-flex justify-content-between mb-2">
    //                                        <span class="text-muted small">Protein</span>
    //                                        <strong class="small">${plan.proteinInGrams}g (${plan.proteinPercentage}%)</strong>
    //                                    </div>
    //                                    <div class="progress mb-2" style="height: 6px;">
    //                                        <div class="progress-bar bg-danger" style="width: 30%;"></div>
    //                                    </div>

    //                                    <div class="d-flex justify-content-between mb-2">
    //                                        <span class="text-muted small">Carbs</span>
    //                                        <strong class="small">${plan.carbInGrams}g (${plan.carbPercentage}%)</strong>
    //                                    </div>
    //                                    <div class="progress mb-2" style="height: 6px;">
    //                                        <div class="progress-bar bg-warning" style="width: 40%;"></div>
    //                                    </div>

    //                                    <div class="d-flex justify-content-between mb-2">
    //                                        <span class="text-muted small">Fats</span>
    //                                        <strong class="small">${plan.fatsInGrams}g (${plan.fatPercentage}%)</strong>
    //                                    </div>
    //                                    <div class="progress" style="height: 6px;">
    //                                        <div class="progress-bar bg-success" style="width: 20%;"></div>
    //                                    </div>
    //                                </div>

    //                                <button class="btn btn-primary-custom btn-sm w-100 view-plan" data-id="${plan.id}">
    //                                    <i class="bi bi-eye me-1"></i> View Full Plan
    //                                </button>
    //                            </div>
    //                        </div>
    //                    </div>
    //                `);
    //            });
    //        },
    //        error: function (xhr) {
    //            console.error("Error loading diet plans:", xhr);
    //            swal("Error", "Failed to load diet plans.", "error");
    //        }
    //    });
    //}
    $('#searchDietPlan').on('keyup', function () {
        let searchText = $(this).val();
        loadDietPlans(searchText);
    });

    // 🟢 دالة تحميل كل الخطط مع إمكانية البحث
    function loadDietPlans(search = "") {
        $.ajax({
            url: '/DietPlan/GetAll',
            method: 'GET',
            data: { search: search }, // 🟢 نمرر كلمة البحث هنا
            success: function (response) {
                $('#dietPlansContainer').empty();

                if (response.data.length === 0) {
                    $('#dietPlansContainer').append(`
                        <div class="text-center text-muted py-4">
                            <i class="bi bi-search"></i> No diet plans found.
                        </div>
                    `);
                    return;
                }


                            response.data.forEach(function (plan) {
                                // Split notes by ,,, delimiter (supports both Arabic and English commas)
                                let notesCards = '';
                                
                                console.log('Plan Notes:', plan.Notes); // Debug log
                                
                                if (plan.Notes && plan.Notes.trim() !== '') {
                                    // Try splitting with different delimiter patterns
                                    let segments = [];
                                    
                                    // Check for various comma patterns (with or without spaces)
                                    if (plan.Notes.includes(',,,')) {
                                        // Split by ,,, and handle spaces
                                        segments = plan.Notes.split(/\s*,,,\s*/);
                                        console.log('Split by English commas (with spaces):', segments.length, 'segments');
                                    } else if (plan.Notes.includes('،،،')) {
                                        // Split by Arabic commas and handle spaces
                                        segments = plan.Notes.split(/\s*،،،\s*/);
                                        console.log('Split by Arabic commas (with spaces):', segments.length, 'segments');
                                    } else {
                                        // If no delimiter found, show as single card
                                        segments = [plan.Notes];
                                        console.log('No delimiter found, showing as single card');
                                    }
                                    
                                    segments = segments.filter(segment => segment.trim() !== '');
                                    
                                    segments.forEach((segment, index) => {
                                        notesCards += `
                                            <div class="diet-notes-card mb-2">
                                                <div class="diet-notes-content">
                                                    ${segment.trim()}
                                                </div>
                                            </div>
                                        `;
                                    });
                                } else {
                                    notesCards = '<div class="text-muted small">No notes available</div>';
                                    console.log('No notes found for this plan');
                                }

                                $('#dietPlansContainer').append(`
                                    <div class="col-lg-4 col-md-6 mb-4">
                                        <div class="card-custom h-100 shadow-sm">
                                            <div class="card-body-custom">
                                                <div class="d-flex justify-content-between align-items-start mb-3">
                                                    <div>
                                                        <h5 class="fw-bold mb-1">${plan.Name}</h5>
                                                        <small class="text-muted">${plan.TotalCal} calories/day</small>
                                                    </div>
                                                    <div class="dropdown">
                                                        <button class="btn btn-sm btn-outline-custom" data-bs-toggle="dropdown">
                                                            <i class="bi bi-three-dots-vertical"></i>
                                                        </button>
                                                        <ul class="dropdown-menu">
                                                            <li><a class="dropdown-item view-plan" data-id="${plan.Id}" href="#"><i class="bi bi-eye me-2"></i>View Details</a></li>
                                                            <li><a class="dropdown-item edit-plan" data-id="${plan.Id}" href="#"><i class="bi bi-pencil me-2"></i>Edit</a></li>
                                                            <li><a class="dropdown-item assign-plan" data-id="${plan.Id}" href="#"><i class="bi bi-person-plus me-2"></i>Assign to Client</a></li>
                                                            <li><hr class="dropdown-divider"></li>
                                                            <li><a class="dropdown-item delete-plan text-danger" data-id="${plan.Id}" href="#"><i class="bi bi-trash me-2"></i>Delete</a></li>
                                                        </ul>
                                                    </div>
                                                </div>

                                                <div class="diet-notes-section mb-3">
                                                    ${notesCards}
                                                </div>

                                                <div class="mb-3">
                                                    <div class="d-flex justify-content-between mb-2">
                                                        <span class="text-muted small">Protein</span>
                                                        <strong class="small">${plan.ProteinInGrams}g (${plan.ProteinPercentage}%)</strong>
                                                    </div>
                                                    <div class="progress mb-2" style="height: 6px;">
                                                        <div class="progress-bar bg-danger" style="width: 30%;"></div>
                                                    </div>

                                                    <div class="d-flex justify-content-between mb-2">
                                                        <span class="text-muted small">Carbs</span>
                                                        <strong class="small">${plan.CarbInGrams}g (${plan.CarbPercentage}%)</strong>
                                                    </div>
                                                    <div class="progress mb-2" style="height: 6px;">
                                                        <div class="progress-bar bg-warning" style="width: 40%;"></div>
                                                    </div>

                                                    <div class="d-flex justify-content-between mb-2">
                                                        <span class="text-muted small">Fats</span>
                                                        <strong class="small">${plan.FatsInGrams}g (${plan.FatPercentage}%)</strong>
                                                    </div>
                                                    <div class="progress" style="height: 6px;">
                                                        <div class="progress-bar bg-success" style="width: 20%;"></div>
                                                    </div>
                                                </div>

                                                <div class="d-flex gap-2">
                                                    <button class="btn btn-primary-custom btn-sm flex-fill view-plan" data-id="${plan.Id}">
                                                        <i class="bi bi-eye me-1"></i> View
                                                    </button>
                                                    <button class="btn btn-success btn-sm flex-fill assign-plan" data-id="${plan.Id}">
                                                        <i class="bi bi-person-plus me-1"></i> Assign
                                                    </button>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                `);
                            });
            },
            error: function (xhr) {
                console.error("Error loading diet plans:", xhr);
                Swal.fire({
                    icon: 'error',
                    title: 'Loading Failed',
                    text: 'Unable to load diet plans. Please refresh the page.',
                    confirmButtonColor: '#ef4444'
                });
            }
        });
    }

    // 🟣 حذف خطة
    $(document).on('click', '.delete-plan', function (e) {
        e.preventDefault();
        e.stopPropagation();
        let id = $(this).data('id');

        Swal.fire({
            title: 'Delete Diet Plan?',
            text: "This action cannot be undone. The diet plan will be permanently removed.",
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
                    text: 'Please wait while we remove the diet plan',
                    allowOutsideClick: false,
                    allowEscapeKey: false,
                    didOpen: () => {
                        Swal.showLoading();
                    }
                });

                $.ajax({
                    url: `/DietPlan/Delete/${id}`,
                    method: 'POST',
                    success: function (res) {
                        Swal.fire({
                            icon: 'success',
                            title: 'Deleted Successfully!',
                            text: res.message || 'Diet plan has been removed.',
                            confirmButtonColor: '#10b981',
                            timer: 2000,
                            showConfirmButton: true
                        });
                        loadDietPlans();
                        loadStatistics();
                    },
                    error: function (xhr) {
                        Swal.fire({
                            icon: 'error',
                            title: 'Deletion Failed',
                            text: 'Unable to delete the diet plan. Please try again.',
                            confirmButtonColor: '#ef4444'
                        });
                    }
                });
            }
        });
    });

    // 🟢 عرض تفاصيل خطة
    $(document).on('click', '.view-plan', function (e) {
        e.preventDefault();
        let id = $(this).data('id');

        $.ajax({
            url: `/DietPlan/GetById/${id}`,
            method: 'GET',
            success: function (plan) {
                $('#viewDietPlanModal .modal-title').text(plan.Goal + " Plan");
                $('#viewDietPlanModal small').text(`${plan.TotalCal} calories/day • Protein: ${plan.ProteinInGrams}g • Carbs: ${plan.CarbInGrams}g • Fats: ${plan.FatsInGrams}g`);
                $('#viewDietPlanModal').modal('show');
            },
            error: function () {
                Swal.fire({
                    icon: 'error',
                    title: 'Loading Failed',
                    text: 'Unable to load diet plan details.',
                    confirmButtonColor: '#ef4444'
                });
            }
        });
    });

    // 🟢 إضافة دايت بلان جديد
    $('#saveDietPlanBtn').on('click', function () {

        const dietPlan = {
            TotalCal: parseFloat($('#totalCal').val()),
            ProteinInGrams: parseFloat($('#protein').val()),
            CarbInGrams: parseFloat($('#carbs').val()),
            FatsInGrams: parseFloat($('#fats').val()),
            Goal: $('#goal').val(),
            Name: $('#planName').val(),
            Age: parseInt($('#age').val()),
            Gender: $('#gender').val(),
            ClientId: $('#clientId').val(), // 🟢 مهم جدًا
            ActivityMultiplier: parseFloat($('#activityMultiplier').val() || 1.2),
            Weight: parseFloat($('#weight').val()),
            Height: parseFloat($('#height').val()),
            Notes: $('#notes').val() || '' // ✅ إضافة حقل الـ Notes

        };

        if (!dietPlan.Name || !dietPlan.Goal || !dietPlan.TotalCal) {
            Swal.fire({
                icon: 'warning',
                title: 'Missing Information',
                text: 'Please fill in all required fields (Name, Goal, and Calories).',
                confirmButtonColor: '#f59e0b'
            });
            return;
        }

        $.ajax({
            url: '/DietPlan/Add',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(dietPlan),
            success: function (res) {
                if (res.success) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Diet Plan Created!',
                        text: res.message || 'Your diet plan has been created successfully.',
                        confirmButtonColor: '#10b981',
                        timer: 2500
                    });
                    $('#createDietPlanModal').modal('hide');
                    $('#dietPlanForm')[0].reset();
                    loadDietPlans();
                    loadStatistics();
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Creation Failed',
                        text: res.message || 'Failed to add diet plan!',
                        confirmButtonColor: '#ef4444'
                    });
                }
            },
            error: function (xhr) {
                console.error(xhr);
                Swal.fire({
                    icon: 'error',
                    title: 'Error Occurred',
                    text: 'An error occurred while adding the diet plan. Please try again.',
                    confirmButtonColor: '#ef4444'
                });
            }
        });
    });

    // 🟢 فتح مودال التعديل
    $(document).on('click', '.edit-plan', function (e) {
        e.preventDefault();
        e.stopPropagation(); // مهم عشان dropdown

        let id = $(this).data('id');

        $.ajax({
            url: `/DietPlan/GetById/${id}`,
            method: 'GET',
            success: function (plan) {
                $('#editPlanId').val(plan.Id);
                $('#editPlanName').val(plan.Name);
                $('#editGoal').val(plan.Goal);
                $('#editTotalCal').val(plan.TotalCal);
                $('#editProtein').val(plan.ProteinInGrams);
                $('#editCarbs').val(plan.CarbInGrams);
                $('#editFats').val(plan.FatsInGrams);
                $('#editage').val(plan.Age);
                $('#editgender').val(plan.Gender);
                $('#editweight').val(plan.Weight);
                $('#editheight').val(plan.Height);
                $('#editNotes').val(plan.Notes || ''); // ✅ تحميل الـ Notes

                $('#editDietPlanModal').modal('show');
            },
            error: function () {
                Swal.fire({
                    icon: 'error',
                    title: 'Loading Failed',
                    text: 'Unable to load diet plan for editing.',
                    confirmButtonColor: '#ef4444'
                });
            }
        });
    });

    // 🟢 تحديث خطة الدايت
    $('#updateDietPlanBtn').on('click', function () {
        const dietPlan = {
            Id: parseInt($('#editPlanId').val()),
            Name: $('#editPlanName').val(),
            Goal: $('#editGoal').val(),
            TotalCal: parseFloat($('#editTotalCal').val()),
            ProteinInGrams: parseFloat($('#editProtein').val()),
            CarbInGrams: parseFloat($('#editCarbs').val()),
            FatsInGrams: parseFloat($('#editFats').val()),
            Age: parseInt($('#editage').val()),
            Gender: $('#editgender').val(),
            Weight: parseFloat($('#editweight').val()),
            Height: parseFloat($('#editheight').val()),
            Notes: $('#editNotes').val() || '' // ✅ إضافة حقل الـ Notes

        };

        if (!dietPlan.Name || !dietPlan.Goal || !dietPlan.TotalCal) {
            Swal.fire({
                icon: 'warning',
                title: 'Missing Information',
                text: 'Please fill in all required fields (Name, Goal, and Calories).',
                confirmButtonColor: '#f59e0b'
            });
            return;
        }

        $.ajax({
            url: '/DietPlan/Update',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(dietPlan),
            success: function (res) {
                if (res.success) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Diet Plan Updated!',
                        text: res.message || 'Your diet plan has been updated successfully.',
                        confirmButtonColor: '#10b981',
                        timer: 2500
                    });
                    $('#editDietPlanModal').modal('hide');
                    loadDietPlans();
                    loadStatistics();
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Update Failed',
                        text: res.message || 'Failed to update diet plan!',
                        confirmButtonColor: '#ef4444'
                    });
                }
            },
            error: function () {
                Swal.fire({
                    icon: 'error',
                    title: 'Error Occurred',
                    text: 'An error occurred while updating the diet plan. Please try again.',
                    confirmButtonColor: '#ef4444'
                });
            }
        });
    });

    // 🟢 Enhanced Features Initialization
    function initializeEnhancedFeatures() {
        // Refresh button
        $('#refreshPlansBtn').on('click', function() {
            $(this).html('<i class="bi bi-hourglass-split me-2"></i>Refreshing...');
            loadDietPlans();
            setTimeout(() => {
                $(this).html('<i class="bi bi-arrow-clockwise me-2"></i>Refresh');
            }, 1000);
        });

        // Filter functionality
        $('.filter-option').on('click', function(e) {
            e.preventDefault();
            const filter = $(this).data('filter');
            applyFilter(filter);
        });

        // Sort functionality
        $('.sort-option').on('click', function(e) {
            e.preventDefault();
            const sort = $(this).data('sort');
            applySorting(sort);
        });

        // View toggle
        $('#gridViewBtn, #listViewBtn').on('click', function() {
            $('.btn-group .btn').removeClass('active');
            $(this).addClass('active');
            
            if ($(this).attr('id') === 'listViewBtn') {
                $('#dietPlansContainer').removeClass('row g-4').addClass('list-view');
            } else {
                $('#dietPlansContainer').removeClass('list-view').addClass('row g-4');
            }
        });
    }

    // 🟢 Load Statistics
    function loadStatistics() {
        $.ajax({
            url: '/DietPlan/GetAll',
            method: 'GET',
            success: function(response) {
                if (response.data) {
                    const plans = response.data;
                    const activePlans = plans.filter(p => p.IsActive).length;
                    const avgCalories = plans.length > 0 ? 
                        Math.round(plans.reduce((sum, p) => sum + p.TotalCal, 0) / plans.length) : 0;
                    
                    $('#activePlansCount').text(activePlans);
                    $('#avgCaloriesCount').text(avgCalories);
                }
            }
        });
    }

    // 🟢 Apply Filter
    function applyFilter(filter) {
        const cards = $('#dietPlansContainer .col-lg-4');
        
        cards.each(function() {
            const card = $(this);
            const planName = card.find('h5').text().toLowerCase();
            
            switch(filter) {
                case 'weight-loss':
                    card.toggle(planName.includes('weight') || planName.includes('loss'));
                    break;
                case 'muscle-gain':
                    card.toggle(planName.includes('muscle') || planName.includes('gain'));
                    break;
                case 'maintenance':
                    card.toggle(planName.includes('maintenance'));
                    break;
                default:
                    card.show();
            }
        });
    }

    // 🟢 Apply Sorting
    function applySorting(sort) {
        const container = $('#dietPlansContainer');
        const cards = container.children('.col-lg-4').get();
        
        cards.sort(function(a, b) {
            let aVal, bVal;
            
            switch(sort) {
                case 'name':
                    aVal = $(a).find('h5').text();
                    bVal = $(b).find('h5').text();
                    return aVal.localeCompare(bVal);
                case 'calories':
                    aVal = parseInt($(a).find('small').text().match(/\d+/)[0]);
                    bVal = parseInt($(b).find('small').text().match(/\d+/)[0]);
                    return bVal - aVal; // Descending
                default:
                    return 0;
            }
        });
        
        container.empty().append(cards);
    }

    // 🟢 Assign Plan to Client
    $(document).on('click', '.assign-plan', function(e) {
        e.preventDefault();
        e.stopPropagation();
        
        const planId = $(this).data('id');
        
        // Show loading while fetching plan details
        Swal.fire({
            title: 'Loading...',
            text: 'Fetching diet plan details',
            allowOutsideClick: false,
            allowEscapeKey: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });
        
        // Load plan details
        $.ajax({
            url: `/DietPlan/GetById/${planId}`,
            method: 'GET',
            success: function(plan) {
                Swal.close();
                $('#assignPlanId').val(plan.Id);
                $('#assignPlanName').text(plan.Name);
                $('#assignPlanDetails').text(`${plan.TotalCal} calories/day • ${plan.Goal}`);
                
                // Load clients
                loadClientsForAssign();
                
                $('#assignDietPlanModal').modal('show');
            },
            error: function() {
                Swal.fire({
                    icon: 'error',
                    title: 'Loading Failed',
                    text: 'Unable to load plan details. Please try again.',
                    confirmButtonColor: '#ef4444'
                });
            }
        });
    });

    // 🟢 Load Clients for Assignment
    function loadClientsForAssign() {
        $.ajax({
            url: '/DietPlan/GetCoachClients',
            method: 'GET',
            success: function(clients) {
                const selector = $('#assignClientSelector');
                selector.empty().append('<option value="">Select a client...</option>');
                
                if (clients && clients.length > 0) {
                    clients.forEach(client => {
                        selector.append(`<option value="${client.Id}">${client.Name}</option>`);
                    });
                } else {
                    selector.append('<option value="" disabled>No clients available</option>');
                }
            },
            error: function() {
                Swal.fire({
                    icon: 'error',
                    title: 'Loading Failed',
                    text: 'Unable to load your clients. Please try again.',
                    confirmButtonColor: '#ef4444'
                });
            }
        });
    }

    // 🟢 Confirm Assignment
    $('#confirmAssignDietPlanBtn').on('click', function() {
        const planId = $('#assignPlanId').val();
        const clientId = $('#assignClientSelector').val();
        
        if (!clientId) {
            Swal.fire({
                icon: 'warning',
                title: 'No Client Selected',
                text: 'Please select a client to assign this diet plan.',
                confirmButtonColor: '#f59e0b'
            });
            return;
        }
        
        const $btn = $(this);
        $btn.prop('disabled', true).html('<i class="bi bi-hourglass-split me-2"></i>Assigning...');
        
        $.ajax({
            url: '/DietPlan/AssignPlan',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ PlanId: parseInt(planId), ClientId: clientId }),
            success: function(response) {
                if (response.success) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Assigned Successfully!',
                        text: response.message || 'Diet plan has been assigned to the client.',
                        confirmButtonColor: '#10b981',
                        timer: 2500,
                        showConfirmButton: true
                    });
                    $('#assignDietPlanModal').modal('hide');
                    loadDietPlans();
                    loadStatistics();
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Assignment Failed',
                        text: response.message || 'Unable to assign the diet plan.',
                        confirmButtonColor: '#ef4444'
                    });
                }
            },
            error: function(xhr) {
                let errorMessage = 'An error occurred while assigning the diet plan.';
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    errorMessage = xhr.responseJSON.message;
                }
                Swal.fire({
                    icon: 'error',
                    title: 'Assignment Failed',
                    text: errorMessage,
                    confirmButtonColor: '#ef4444'
                });
            },
            complete: function() {
                $btn.prop('disabled', false).html('<i class="bi bi-check-circle me-2"></i>Assign Plan');
            }
        });
    });

});


