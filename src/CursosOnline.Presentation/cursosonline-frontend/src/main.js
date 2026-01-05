import './style.css'

// Ajusta estas URLs si tu API corre en otro puerto u host
// Aquí usamos el perfil http del backend: http://localhost:5066
const COURSES_API_URL = 'http://localhost:5066/api/courses';
const COURSES_SEARCH_URL = `${COURSES_API_URL}/search`;
const AUTH_LOGIN_URL = 'http://localhost:5066/api/auth/login';

const loginPanel = document.querySelector('#login-panel');
const coursePanel = document.querySelector('#course-panel');
const coursesPanel = document.querySelector('#courses-panel');
const lessonsPanel = document.querySelector('#lessons-panel');

const loginForm = document.querySelector('#login-form');
const emailInput = document.querySelector('#email');
const passwordInput = document.querySelector('#password');

const logoutButton = document.querySelector('#logout-button');

const courseForm = document.querySelector('#course-form');
const titleInput = document.querySelector('#title');
const tableBody = document.querySelector('#courses-body');
const messageBox = document.querySelector('#message');

// Filtros y paginación de cursos
const searchInput = document.querySelector('#search-input');
const statusFilter = document.querySelector('#status-filter');
const prevPageButton = document.querySelector('#prev-page');
const nextPageButton = document.querySelector('#next-page');
const pageInfo = document.querySelector('#page-info');

let currentPage = 1;
const pageSize = 5;
let lastTotalCount = 0;

// Gestión de lecciones
const lessonsMessageBox = document.querySelector('#lessons-message');
const lessonsBody = document.querySelector('#lessons-body');
const lessonForm = document.querySelector('#lesson-form');
const lessonTitleInput = document.querySelector('#lesson-title');
const closeLessonsButton = document.querySelector('#close-lessons');
let currentCourseIdForLessons = null;

function getToken() {
  return localStorage.getItem('token');
}

function setToken(token) {
  if (token) {
    localStorage.setItem('token', token);
  } else {
    localStorage.removeItem('token');
  }
}

function applyAuthState() {
  const token = getToken();
  const isAuthenticated = !!token;

  loginPanel.hidden = isAuthenticated;
  coursePanel.hidden = !isAuthenticated;
  coursesPanel.hidden = !isAuthenticated;

  if (!isAuthenticated) {
    lessonsPanel.hidden = true;
  }
}

function showMessage(text, type = 'success') {
  if (!messageBox) return;
  messageBox.hidden = false;
  messageBox.textContent = text;
  messageBox.className = `message message--${type}`;

  setTimeout(() => {
    messageBox.hidden = true;
  }, 3000);
}

async function fetchCourses() {
  try {
    const token = getToken();
    if (!token) {
      applyAuthState();
      return;
    }

    const q = (searchInput?.value ?? '').trim();
    const status = statusFilter?.value ?? '';

    const params = new URLSearchParams();
    if (q) params.append('q', q);
    if (status) params.append('status', status);
    params.append('page', currentPage.toString());
    params.append('pageSize', pageSize.toString());

    const url = `${COURSES_SEARCH_URL}?${params.toString()}`;

    const response = await fetch(url, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });
    if (!response.ok) {
      throw new Error('Error al obtener los cursos');
    }

    const result = await response.json();
    const courses = result.data ?? result.Data ?? [];
    const totalCount = result.totalCount ?? result.TotalCount ?? 0;
    lastTotalCount = totalCount;

    renderCourses(courses);
    updatePagination();
  } catch (error) {
    console.error(error);
    showMessage('No se pudieron cargar los cursos', 'error');
  }
}

function renderCourses(courses) {
  tableBody.innerHTML = '';

  if (!courses.length) {
    const row = document.createElement('tr');
    const cell = document.createElement('td');
    cell.colSpan = 3;
    cell.textContent = 'No hay cursos todavía';
    row.appendChild(cell);
    tableBody.appendChild(row);
    return;
  }

  courses.forEach((course) => {
    const row = document.createElement('tr');

    const titleCell = document.createElement('td');
    titleCell.textContent = course.title ?? course.Title;

    const statusCell = document.createElement('td');
    const statusValue = (course.status ?? course.Status ?? '').toString();
    const badge = document.createElement('span');
    badge.classList.add('badge');
    if (statusValue.toLowerCase() === 'published') {
      badge.classList.add('badge--published');
      badge.textContent = 'Publicado';
    } else {
      badge.classList.add('badge--draft');
      badge.textContent = 'Borrador';
    }
    statusCell.appendChild(badge);

    const actionsCell = document.createElement('td');
    actionsCell.classList.add('actions');

    const lessonsButton = document.createElement('button');
    lessonsButton.textContent = 'Lecciones';
    lessonsButton.addEventListener('click', () => openLessons(course.id ?? course.Id));

    const togglePublishButton = document.createElement('button');
    togglePublishButton.textContent = statusValue.toLowerCase() === 'published' ? 'Despublicar' : 'Publicar';
    togglePublishButton.addEventListener('click', () => togglePublish(course.id ?? course.Id, statusValue));

    const deleteButton = document.createElement('button');
    deleteButton.textContent = 'Eliminar';
    deleteButton.classList.add('actions__delete');
    deleteButton.addEventListener('click', () => deleteCourse(course.id ?? course.Id));

    actionsCell.appendChild(lessonsButton);
    actionsCell.appendChild(togglePublishButton);
    actionsCell.appendChild(deleteButton);

    row.appendChild(titleCell);
    row.appendChild(statusCell);
    row.appendChild(actionsCell);

    tableBody.appendChild(row);
  });
}

async function createCourse(title) {
  try {
    const token = getToken();
    if (!token) {
      applyAuthState();
      return;
    }

    const response = await fetch(COURSES_API_URL, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify({ title }),
    });

    if (!response.ok) {
      throw new Error('Error al crear el curso');
    }

    showMessage('Curso creado correctamente', 'success');
    titleInput.value = '';
    await fetchCourses();
  } catch (error) {
    console.error(error);
    showMessage('No se pudo crear el curso', 'error');
  }
}

async function deleteCourse(id) {
  if (!id) return;

  const confirmDelete = window.confirm('¿Seguro que quieres eliminar este curso?');
  if (!confirmDelete) return;

  try {
    const token = getToken();
    if (!token) {
      applyAuthState();
      return;
    }

    const response = await fetch(`${COURSES_API_URL}/${id}`, {
      method: 'DELETE',
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });

    if (!response.ok) {
      throw new Error('Error al eliminar el curso');
    }

    showMessage('Curso eliminado correctamente', 'success');
    await fetchCourses();
  } catch (error) {
    console.error(error);
    showMessage('No se pudo eliminar el curso', 'error');
  }
}

async function togglePublish(id, currentStatus) {
  if (!id) return;

  const token = getToken();
  if (!token) {
    applyAuthState();
    return;
  }

  const isPublished = currentStatus.toLowerCase() === 'published';
  const endpoint = isPublished ? 'unpublish' : 'publish';

  try {
    const response = await fetch(`${COURSES_API_URL}/${id}/${endpoint}`, {
      method: 'PATCH',
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(errorText || 'Error al cambiar el estado del curso');
    }

    showMessage(isPublished ? 'Curso despublicado' : 'Curso publicado', 'success');
    await fetchCourses();
  } catch (error) {
    console.error(error);
    showMessage('No se pudo cambiar el estado del curso', 'error');
  }
}

loginForm.addEventListener('submit', async (event) => {
  event.preventDefault();

  const email = emailInput.value.trim();
  const password = passwordInput.value.trim();
  if (!email || !password) return;

  try {
    const response = await fetch(AUTH_LOGIN_URL, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ email, password }),
    });

    if (!response.ok) {
      throw new Error('Error en el inicio de sesión');
    }

    const result = await response.json();
    const token = result.token ?? result.Token;
    if (!token) {
      throw new Error('Token no recibido');
    }

    setToken(token);
    applyAuthState();
    await fetchCourses();
    showMessage('Inicio de sesión correcto', 'success');
  } catch (error) {
    console.error(error);
    setToken(null);
    applyAuthState();
    showMessage('No se pudo iniciar sesión', 'error');
  }
});

courseForm.addEventListener('submit', async (event) => {
  event.preventDefault();
  const title = titleInput.value.trim();
  if (!title) return;

  await createCourse(title);
});

logoutButton.addEventListener('click', () => {
  setToken(null);
  applyAuthState();
  tableBody.innerHTML = '';
  lessonsBody.innerHTML = '';
});

// Eventos de filtros y paginación
if (searchInput) {
  searchInput.addEventListener('input', () => {
    currentPage = 1;
    fetchCourses();
  });
}

if (statusFilter) {
  statusFilter.addEventListener('change', () => {
    currentPage = 1;
    fetchCourses();
  });
}

prevPageButton?.addEventListener('click', () => {
  if (currentPage > 1) {
    currentPage -= 1;
    fetchCourses();
  }
});

nextPageButton?.addEventListener('click', () => {
  const maxPage = Math.max(1, Math.ceil(lastTotalCount / pageSize));
  if (currentPage < maxPage) {
    currentPage += 1;
    fetchCourses();
  }
});

// Lecciones
function showLessonsMessage(text, type = 'success') {
  if (!lessonsMessageBox) return;
  lessonsMessageBox.hidden = false;
  lessonsMessageBox.textContent = text;
  lessonsMessageBox.className = `message message--${type}`;

  setTimeout(() => {
    lessonsMessageBox.hidden = true;
  }, 3000);
}

async function openLessons(courseId) {
  currentCourseIdForLessons = courseId;
  lessonsPanel.hidden = false;
  await fetchLessons();
}

async function fetchLessons() {
  if (!currentCourseIdForLessons) return;

  const token = getToken();
  if (!token) {
    applyAuthState();
    return;
  }

  try {
    const response = await fetch(`${COURSES_API_URL}/${currentCourseIdForLessons}/lessons`, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });

    if (!response.ok) {
      throw new Error('Error al obtener las lecciones');
    }

    const result = await response.json();
    const lessons = result.data ?? result.Data ?? [];

    renderLessons(lessons);
  } catch (error) {
    console.error(error);
    showLessonsMessage('No se pudieron cargar las lecciones', 'error');
  }
}

function renderLessons(lessons) {
  lessonsBody.innerHTML = '';

  if (!lessons.length) {
    const row = document.createElement('tr');
    const cell = document.createElement('td');
    cell.colSpan = 3;
    cell.textContent = 'No hay lecciones todavía';
    row.appendChild(cell);
    lessonsBody.appendChild(row);
    return;
  }

  lessons.forEach((lesson) => {
    const row = document.createElement('tr');

    const titleCell = document.createElement('td');
    titleCell.textContent = lesson.title ?? lesson.Title;

    const orderCell = document.createElement('td');
    orderCell.textContent = lesson.order ?? lesson.Order;

    const actionsCell = document.createElement('td');
    actionsCell.classList.add('actions');

    const upButton = document.createElement('button');
    upButton.textContent = '↑';
    upButton.addEventListener('click', () => moveLesson(lesson.id ?? lesson.Id, 'move-up'));

    const downButton = document.createElement('button');
    downButton.textContent = '↓';
    downButton.addEventListener('click', () => moveLesson(lesson.id ?? lesson.Id, 'move-down'));

    const deleteButton = document.createElement('button');
    deleteButton.textContent = 'Eliminar';
    deleteButton.classList.add('actions__delete');
    deleteButton.addEventListener('click', () => deleteLesson(lesson.id ?? lesson.Id));

    actionsCell.appendChild(upButton);
    actionsCell.appendChild(downButton);
    actionsCell.appendChild(deleteButton);

    row.appendChild(titleCell);
    row.appendChild(orderCell);
    row.appendChild(actionsCell);

    lessonsBody.appendChild(row);
  });
}

async function createLesson(title) {
  if (!currentCourseIdForLessons) return;

  const token = getToken();
  if (!token) {
    applyAuthState();
    return;
  }

  try {
    const response = await fetch(`${COURSES_API_URL}/${currentCourseIdForLessons}/lessons`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify({ title }),
    });

    if (!response.ok) {
      throw new Error('Error al crear la lección');
    }

    showLessonsMessage('Lección creada correctamente', 'success');
    lessonTitleInput.value = '';
    await fetchLessons();
  } catch (error) {
    console.error(error);
    showLessonsMessage('No se pudo crear la lección', 'error');
  }
}

async function deleteLesson(id) {
  if (!currentCourseIdForLessons || !id) return;

  const confirmDelete = window.confirm('¿Seguro que quieres eliminar esta lección?');
  if (!confirmDelete) return;

  const token = getToken();
  if (!token) {
    applyAuthState();
    return;
  }

  try {
    const response = await fetch(`${COURSES_API_URL}/${currentCourseIdForLessons}/lessons/${id}`, {
      method: 'DELETE',
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });

    if (!response.ok) {
      throw new Error('Error al eliminar la lección');
    }

    showLessonsMessage('Lección eliminada correctamente', 'success');
    await fetchLessons();
  } catch (error) {
    console.error(error);
    showLessonsMessage('No se pudo eliminar la lección', 'error');
  }
}

async function moveLesson(id, direction) {
  if (!currentCourseIdForLessons || !id) return;

  const token = getToken();
  if (!token) {
    applyAuthState();
    return;
  }

  try {
    const response = await fetch(`${COURSES_API_URL}/${currentCourseIdForLessons}/lessons/${id}/${direction}`, {
      method: 'PATCH',
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });

    if (!response.ok) {
      throw new Error('Error al reordenar la lección');
    }

    await fetchLessons();
  } catch (error) {
    console.error(error);
    showLessonsMessage('No se pudo reordenar la lección', 'error');
  }
}

lessonForm?.addEventListener('submit', async (event) => {
  event.preventDefault();
  const title = lessonTitleInput.value.trim();
  if (!title) return;

  await createLesson(title);
});

closeLessonsButton?.addEventListener('click', () => {
  lessonsPanel.hidden = true;
  currentCourseIdForLessons = null;
  lessonsBody.innerHTML = '';
});

function updatePagination() {
  const maxPage = Math.max(1, Math.ceil(lastTotalCount / pageSize));
  if (pageInfo) {
    pageInfo.textContent = `Página ${currentPage} de ${maxPage}`;
  }

  if (prevPageButton) {
    prevPageButton.disabled = currentPage <= 1;
  }

  if (nextPageButton) {
    nextPageButton.disabled = currentPage >= maxPage;
  }
}

// Estado inicial
applyAuthState();

// Si ya hay token, intentar cargar cursos
if (getToken()) {
  fetchCourses();
}
