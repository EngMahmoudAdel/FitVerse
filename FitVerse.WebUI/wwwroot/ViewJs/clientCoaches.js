$(document).ready(function () {
    loadCoachesWithSpecialties();

    // ✅ البحث أثناء الكتابة
    $('#searchCoach').on('keyup', function () {
        let searchText = $(this).val().toLowerCase();
        $('.coach-card').each(function () {
            let name = $(this).find('.coach-name').text().toLowerCase();
            let specialty = $(this).find('.coach-specialty').text().toLowerCase();

            if (name.includes(searchText) || specialty.includes(searchText)) {
                $(this).parent().show();
            } else {
                $(this).parent().hide();
            }
        });
    });
});

function loadCoachesWithSpecialties() {
    $.ajax({
        url: '/Coach/GetAllCoaches',
        method: 'GET',
        success: function (response) {
            if (!response.success || !response.data || response.data.length === 0) {
                $('#coachesContainer').html(`
                    <div class="text-center text-muted py-5">
                        <i class="bi bi-person-x fs-1 d-block mb-3"></i>
                        <p>No coaches found.</p>
                    </div>
                `);
                return;
            }

            $('#coachesContainer').empty();

            response.data.forEach(coach => {
                let imageUrl;
                if (coach.ImagePath && coach.ImagePath.trim() !== '') {
                    if (coach.ImagePath.startsWith('/')) {
                        imageUrl = coach.ImagePath;
                    } else {
                        imageUrl = `/profile-image/${coach.ImagePath}`;
                    }
                } else {
                    imageUrl = `https://ui-avatars.com/api/?name=${encodeURIComponent(coach.Name)}&background=6366f1&color=fff`;
                }

                let specialtiesText = coach.Specialties && coach.Specialties.length > 0
                    ? coach.Specialties.join(', ')
                    : 'General';

                let card = `
                    <div class="col-lg-4 col-md-6">
                        <div class="card-custom h-100 coach-card">
                            <div class="card-body-custom text-center">
                                <img src="${imageUrl}" alt="${coach.Name}" 
                                     style="width: 100px; height: 100px; border-radius: 50%; margin-bottom: 16px; object-fit: cover;">
                                
                                <h5 class="fw-bold mb-2 coach-name">${coach.Name}</h5>

                                <div class="mb-2">
                                    <span class="badge-custom badge-primary mb-3 coach-specialty">
                                        ${specialtiesText}
                                    </span>
                                </div>

                                <p class="text-muted small mb-3">${coach.About ?? 'No description available.'}</p>

                                <div class="d-flex justify-content-around mb-3 py-3" 
                                     style="border-top: 1px solid #e5e7eb; border-bottom: 1px solid #e5e7eb;">
                                    <div>
                                        <div class="fw-bold">${coach.ExperienceYears ?? 0}</div>
                                        <small class="text-muted">Years Experience.</small>
                                    </div>
                                    <div>
                                        <div class="badge-custom mb-3 ${coach.IsActive ? 'badge-success' : 'badge-danger'}">
                                            ${coach.IsActive ? 'Active' : 'Not Active'}
                                        </div>  
                                    </div>
                                </div>

                                <button class="btn btn-primary-custom w-100 view-packages"
                                        data-bs-toggle="modal" 
                                        data-bs-target="#coachPackagesModal"
                                        data-coach-id="${coach.Id}" 
                                        data-coach-name="${coach.Name}">
                                    <i class="bi bi-box-seam me-2"></i> View Packages
                                </button>
                            </div>
                        </div>
                    </div>
                `;

                $('#coachesContainer').append(card);
            });
        },
        error: function () {
            $('#coachesContainer').html(`
                <div class="text-center text-danger py-5">
                    <i class="bi bi-exclamation-triangle fs-1 d-block mb-3"></i>
                    <p>Failed to load coaches. Please try again later.</p>
                </div>
            `);
        }
    });
}

$(document).on("click", ".view-packages", function () {
    let coachId = $(this).data("coach-id");
    let coachName = $(this).data("coach-name");

    $("#coachNameTitle").text(`${coachName}'s Packages`);
    $("#packagesContainer").html('<div class="text-center py-5">Loading packages...</div>');

    $.ajax({
        url: `/Coach/GetPackagesByCoachId?coachId=${coachId}`,
        type: "GET",
        success: function (response) {
            console.log('Packages response:', response); // Debug log
            
            let packages = response.data || [];
            let html = "";

            if (!response.success && response.message) {
                html = `<div class="text-center py-5 text-warning">${response.message}</div>`;
            } else if (packages.length === 0) {
                html = `<div class="text-center py-5 text-muted">No packages found for this coach.</div>`;
            } else {
                packages.forEach(p => {
                    // استخدم الـ Description لتقسيم الـ Features
                    let features = p.Description ? p.Description.split(',') : [];

                    let featureList = '<ul class="list-unstyled text-start mb-4">';
                    features.forEach(f => {
                        featureList += `
                            <li class="mb-2">
                                <i class="bi bi-check-circle-fill text-success me-2"></i> ${f}
                            </li>`;
                    });
                    featureList += '</ul>';

                    html += `
                        <div class="col-md-4">
                            <div class="card-custom h-100">
                                <div class="card-body-custom text-center">
                                    <h5 class="fw-bold mb-3">${p.Name}</h5>
                                    <div class="mb-3">
                                        <h2 class="fw-bold mb-0" style="color: #6366f1;">$${p.Price}</h2>
                                        <small class="text-muted">/month</small>
                                    </div>
                                    ${featureList}
                                    <a href="/Client/Payment?packageId=${p.Id}&coachId=${coachId}" 
                                       class="btn btn-outline-custom w-100">
                                       Select Package
                                    </a>
                                </div>
                            </div>
                        </div>
                    `;
                });
            }

            $("#packagesContainer").html(html);
            $("#coachPackagesModal").modal("show");
        },
        error: function (xhr, status, error) {
            console.error('Error loading packages:', error); // Debug log
            $("#packagesContainer").html('<div class="text-center text-danger py-5">Error loading packages. Please try again later.</div>');
        }
    });
});
