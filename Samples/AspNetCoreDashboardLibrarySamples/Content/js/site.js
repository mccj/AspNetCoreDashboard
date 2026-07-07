(function () {
    var base = getBasePath();
    var status = document.getElementById("asset-status");
    if (status) {
        status.textContent = "css、js、png 与 woff 已通过 MapEmbeddedUi 加载。";
    }

    bindAction("btn-dashboard-status", function () {
        return requestJson(base + "api/status", "status-output");
    });
    bindAction("btn-diagnostics-status", function () {
        return requestJson("/Diagnostics/api/status", "status-output");
    });
    bindAction("btn-health", function () {
        return requestText(base + "health", "status-output");
    });

    bindAction("btn-item-get", function () {
        return requestJson(base + "api/items/42", "rest-output");
    });
    bindAction("btn-item-put", function () {
        return fetch(base + "api/items/7", {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ name: "updated" })
        }).then(toOutput("rest-output"));
    });
    bindAction("btn-item-patch", function () {
        return fetch(base + "api/items/11", {
            method: "PATCH",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ status: "ok" })
        }).then(toOutput("rest-output"));
    });
    bindAction("btn-item-delete", function () {
        return fetch(base + "api/items/9", { method: "DELETE" })
            .then(function (response) {
                setOutput("rest-output", response.status + " " + (response.headers.get("X-Deleted-Item") || ""));
            });
    });
    bindAction("btn-item-options", function () {
        return fetch(base + "api/items/1", { method: "OPTIONS" })
            .then(function (response) {
                return response.text().then(function (body) {
                    setOutput("rest-output", response.status + " Allow=" + response.headers.get("Allow") + " body=" + JSON.stringify(body));
                });
            });
    });

    bindAction("btn-flow-get", function () {
        var filter = valueOf("flow-filter");
        var orderBy = valueOf("flow-order");
        var url = base + "FlowStatistics?filter=" + encodeURIComponent(filter) + "&orderBy=" + encodeURIComponent(orderBy);
        return requestText(url, "flow-output");
    });
    bindAction("btn-flow-post-urlencoded", function () {
        var body = new URLSearchParams();
        body.set("filter", valueOf("flow-filter"));
        body.set("orderBy", valueOf("flow-order"));
        return fetch(base + "FlowStatistics", { method: "POST", body: body }).then(toOutput("flow-output"));
    });
    bindAction("btn-flow-post-multipart", function () {
        var body = new FormData();
        body.set("filter", valueOf("flow-filter"));
        body.set("orderBy", valueOf("flow-order"));
        return fetch(base + "FlowStatistics", { method: "POST", body: body }).then(toOutput("flow-output"));
    });

    bindAction("btn-upload", function () {
        var input = document.getElementById("upload-file");
        var body = new FormData();
        if (input && input.files && input.files[0]) {
            body.append("file", input.files[0]);
        }
        return fetch(base + "api/upload", { method: "POST", body: body }).then(toOutput("file-output"));
    });

    bindAction("btn-job-refresh", refreshJobs);
    bindAction("btn-job-create", function () {
        return fetch(base + "api/jobs", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ name: valueOf("job-name") })
        }).then(function () {
            return refreshJobs();
        });
    });

    document.getElementById("jobs-body").addEventListener("click", function (event) {
        var target = event.target;
        if (!target || target.tagName !== "BUTTON") {
            return;
        }

        var id = target.getAttribute("data-job-id");
        if (!id) {
            return;
        }

        fetch(base + "api/jobs/" + id, { method: "DELETE" }).then(refreshJobs);
    });

    setupSpaRouting();
    refreshJobs();

    function setupSpaRouting() {
        var tabs = document.querySelectorAll(".tabs a[data-route]");
        for (var i = 0; i < tabs.length; i++) {
            tabs[i].addEventListener("click", function (event) {
                event.preventDefault();
                navigateTo(this.getAttribute("data-route") || "/");
            });
        }

        window.addEventListener("hashchange", syncRouteFromHash);
        window.addEventListener("popstate", syncRouteFromLocation);
        syncRouteFromLocation();
    }

    function navigateTo(route) {
        var normalized = route === "/" ? "/" : route;
        if (window.location.hash) {
            window.location.hash = normalized === "/" ? "#/" : "#" + normalized;
        } else {
            var target = base.replace(/\/$/, "") + (normalized === "/" ? "/" : normalized);
            window.history.pushState({}, "", target);
        }
        renderRoute(normalized);
    }

    function syncRouteFromHash() {
        var hash = window.location.hash.replace(/^#/, "") || "/";
        renderRoute(hash);
    }

    function syncRouteFromLocation() {
        var path = window.location.pathname || "/";
        var prefix = base.replace(/\/$/, "");
        if (path.indexOf(prefix) === 0) {
            var route = path.substring(prefix.length) || "/";
            renderRoute(route);
            return;
        }

        renderRoute("/");
    }

    function renderRoute(route) {
        var home = document.getElementById("view-home");
        var about = document.getElementById("view-about");
        var spaPath = document.getElementById("spa-path");
        var isAbout = route === "/about";

        if (home) {
            home.classList.toggle("hidden", isAbout);
        }
        if (about) {
            about.classList.toggle("hidden", !isAbout);
        }
        if (spaPath) {
            spaPath.textContent = base.replace(/\/$/, "") + route;
        }

        var tabs = document.querySelectorAll(".tabs a[data-route]");
        for (var i = 0; i < tabs.length; i++) {
            var tabRoute = tabs[i].getAttribute("data-route") || "/";
            tabs[i].classList.toggle("active", tabRoute === route);
        }
    }

    function refreshJobs() {
        return requestJson(base + "api/jobs", null).then(function (jobs) {
            var body = document.getElementById("jobs-body");
            if (!body) {
                return;
            }

            if (!jobs || !jobs.length) {
                body.innerHTML = "<tr><td colspan=\"5\">暂无任务</td></tr>";
                return;
            }

            var html = "";
            for (var i = 0; i < jobs.length; i++) {
                var job = jobs[i];
                html += "<tr>" +
                    "<td>" + escapeHtml(String(job.id)) + "</td>" +
                    "<td>" + escapeHtml(job.name) + "</td>" +
                    "<td>" + escapeHtml(job.status) + "</td>" +
                    "<td>" + escapeHtml(job.createdAt) + "</td>" +
                    "<td><button type=\"button\" data-job-id=\"" + escapeHtml(String(job.id)) + "\">删除</button></td>" +
                    "</tr>";
            }
            body.innerHTML = html;
        });
    }

    function getBasePath() {
        var path = window.location.pathname || "/";
        if (path.slice(-1) !== "/") {
            var slash = path.lastIndexOf("/");
            path = slash >= 0 ? path.substring(0, slash + 1) : "/";
        }
        return path;
    }

    function bindAction(id, handler) {
        var element = document.getElementById(id);
        if (!element) {
            return;
        }
        element.addEventListener("click", function () {
            handler().catch(showError);
        });
    }

    function requestJson(url, outputId) {
        return fetch(url).then(function (response) {
            return response.json().then(function (data) {
                if (outputId) {
                    setOutput(outputId, JSON.stringify(data, null, 2));
                }
                return data;
            });
        });
    }

    function requestText(url, outputId) {
        return fetch(url).then(function (response) {
            return response.text().then(function (text) {
                setOutput(outputId, text);
                return text;
            });
        });
    }

    function toOutput(outputId) {
        return function (response) {
            return response.text().then(function (text) {
                setOutput(outputId, response.status + " " + text);
            });
        };
    }

    function setOutput(id, text) {
        var element = document.getElementById(id);
        if (element) {
            element.textContent = text;
        }
    }

    function valueOf(id) {
        var element = document.getElementById(id);
        return element ? element.value : "";
    }

    function escapeHtml(value) {
        return String(value)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;");
    }

    function showError(error) {
        window.console.error(error);
        alert(error && error.message ? error.message : String(error));
    }
})();
