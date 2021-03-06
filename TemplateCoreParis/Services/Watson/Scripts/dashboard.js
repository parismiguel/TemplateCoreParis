﻿/*
 * Copyright © 2016 I.B.M. All rights reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the “License”);
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an “AS IS” BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

/* The Common module is designed as an auxiliary module
 * to hold functions that are used in multiple other modules
 * and functions that do not fit into the scopes of other modules
 */

/* eslint no-unused-vars: ["error", { "varsIgnorePattern": "^Common$" }] */

var Common = (function () {
    var classes = {
        hide: 'hide',
        fade: 'fade',
        fadeOut: 'fade-out'
    };

    // Publicly accessible methods defined
    return {
        buildDomElement: buildDomElementFromJson,
        wait: wait,
        fireEvent: fireEvent,
        listForEach: listForEach,
        partial: partial,
        hide: hide,
        show: show,
        toggle: toggle,
        fadeOut: fadeOut,
        fadeIn: fadeIn,
        fadeToggle: fadeToggle,
        addClass: addClass,
        removeClass: removeClass,
        toggleClass: toggleClass
    };

    // Take in JSON object and build a DOM element out of it
    // (Limited in scope, cannot necessarily create arbitrary DOM elements)
    // JSON Example:
    //  {
    //    "tagName": "div",
    //    "text": "Hello World!",
    //    "html": "Hello <br> World!", (text preempts html attribute)
    //    "classNames": ["aClass", "bClass"],
    //    "attributes": [{
    //      "name": "onclick",
    //      "value": "alert('Hi there!')"
    //    }],
    //    "children: [{other similarly structured JSON objects...}, {...}]
    //  }
    //
    // Resulting DOM:
    // <div class="aClass bClass" onclick="alert('Hi there!')">
    //   Hello World!
    //   --- children nodes, etc. ---
    // <div>
    function buildDomElementFromJson(domJson) {
        // Create a DOM element with the given tag name
        var element = document.createElement(domJson.tagName);

        // Fill the "content" of the element
        if (domJson.text) {
            element.textContent = domJson.text;
        } else if (domJson.html) {
            element.insertAdjacentHTML('beforeend', domJson.html);
        }

        // Add classes to the element
        if (domJson.classNames) {
            for (var i = 0; i < domJson.classNames.length; i++) {
                Common.addClass(element, domJson.classNames[i]);
            }
        }
        // Add attributes to the element
        if (domJson.attributes) {
            for (var j = 0; j < domJson.attributes.length; j++) {
                var currentAttribute = domJson.attributes[j];
                element.setAttribute(currentAttribute.name, currentAttribute.value);
            }
        }
        // Add children elements to the element
        if (domJson.children) {
            for (var k = 0; k < domJson.children.length; k++) {
                var currentChild = domJson.children[k];
                element.appendChild(buildDomElementFromJson(currentChild));
            }
        }
        return element;
    }

    // Wait until a condition is true until running a function
    // (poll based on interval in ms)
    function wait(conditionFunction, execFunction, interval) {
        if (!conditionFunction()) {
            setTimeout(function () {
                wait(conditionFunction, execFunction, interval);
            }, interval);
        } else {
            execFunction();
        }
    }

    // Triggers an event of the given type on the given object
    function fireEvent(element, event) {
        var evt;
        if (document.createEventObject) {
            // dispatch for IE
            evt = document.createEventObject();
            return element.fireEvent('on' + event, evt);
        }
        // otherwise, dispatch for Firefox, Chrome + others
        evt = document.createEvent('HTMLEvents');
        evt.initEvent(event, true, true); // event type,bubbling,cancelable
        return !element.dispatchEvent(evt);
    }

    // A function that runs a for each loop on a List, running the callback function for each one
    function listForEach(list, callback) {
        for (var i = 0; i < list.length; i++) {
            callback.call(null, list[i]);
        }
    }

    function partial(func /* , any number of bound args...*/) {
        var sliceFunc = Array.prototype.slice;
        var args = sliceFunc.call(arguments, 1);
        return function () {
            return func.apply(this, args.concat(sliceFunc.call(arguments, 0)));
        };
    }

    // Adds the 'hide' class to a given element, giving it a CSS display value of 'none'
    function hide(element) {
        addClass(element, classes.hide);
    }

    // Removes the 'hide' class from a given element, removing its CSS display value of 'none'
    function show(element) {
        removeClass(element, classes.hide);
    }

    // Toggles the 'hide' class on a given element, toggling a CSS display value of 'none'
    function toggle(element) {
        toggleClass(element, classes.hide);
    }

    // Causes an element to fade out by adding the 'fade' and 'fade-out' classes
    function fadeOut(element) {
        addClass(element, classes.fade);
        addClass(element, classes.fadeOut);
    }

    // Causes an element to fade back in by adding the 'fade' class and removing the 'fade-out' class
    function fadeIn(element) {
        addClass(element, classes.fade);
        removeClass(element, classes.fadeOut);
    }

    // Causes an element to toggle fading out or back in
    // by adding the 'fade' class and toggling the 'fade-out' class
    function fadeToggle(element) {
        addClass(element, classes.fade);
        toggleClass(element, classes.fadeOut);
    }

    // Auxiliary function for adding a class to an element
    // (to help mitigate IE's lack of support for svg.classList)
    function addClass(element, clazz) {
        if (element.classList) {
            element.classList.add(clazz);
        } else {
            ieSvgAddClass(element, clazz);
        }
    }

    // Auxiliary function for removing a class from an element
    // (to help mitigate IE's lack of support for svg.classList)
    function removeClass(element, clazz) {
        if (element.classList) {
            element.classList.remove(clazz);
        } else {
            ieSvgRemoveClass(element, clazz);
        }
    }

    // Auxiliary function for toggling a class on an element
    // (to help mitigate IE's lack of support for svg.classList)
    function toggleClass(element, clazz) {
        if (element.classList) {
            element.classList.toggle(clazz);
        } else {
            ieSvgToggleClass(element, clazz);
        }
    }

    // Auxiliary function for checking whether an element contains a class
    // (to help mitigate IE's lack of support for svg.classList)
    function ieSvgContainsClass(element, clazz) {
        return (element.className.baseVal.indexOf(clazz) > -1);
    }

    // Auxiliary function for adding a class to an element without using the classList property
    // (to help mitigate IE's lack of support for svg.classList)
    function ieSvgAddClass(element, clazz, bypassCheck) {
        if (bypassCheck || !ieSvgContainsClass(element, clazz)) {
            var classNameValue = element.className.baseVal;
            classNameValue += (' ' + clazz);
            element.className.baseVal = classNameValue;
        }
    }

    // Auxiliary function for removing a class from an element without using the classList property
    // (to help mitigate IE's lack of support for svg.classList)
    function ieSvgRemoveClass(element, clazz) {
        var classNameValue = element.className.baseVal;
        classNameValue = classNameValue.replace(clazz, '');
        element.className.baseVal = classNameValue;
    }

    // Auxiliary function for toggling a class on an element without using the classList property
    // (to help mitigate IE's lack of support for svg.classList)
    function ieSvgToggleClass(element, clazz) {
        if (ieSvgContainsClass(element, clazz)) {
            ieSvgRemoveClass(element, clazz);
        } else {
            ieSvgAddClass(element, clazz, true);
        }
    }
}());

/*
 * Copyright © 2016 I.B.M. All rights reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the “License”);
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an “AS IS” BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

/* The Api module is designed to handle all interactions with the server */

/* eslint no-unused-vars: ["error", { "varsIgnorePattern": "^Api$" }] */

var Api = (function () {
    'use strict';
    var userPayload;
    var watsonPayload;
    var context;

    var messageEndpoint = '/api/message';

    // Publicly accessible methods defined
    return {
        initConversation: initConversation,
        postConversationMessage: postConversationMessage,

        // The request/response getters/setters are defined here to prevent internal methods
        // from calling the methods without any of the callbacks that are added elsewhere.
        getUserPayload: function () {
            return userPayload;
        },
        setUserPayload: function (payload) {
            userPayload = payload;
        },
        getWatsonPayload: function () {
            return watsonPayload;
        },
        setWatsonPayload: function (payload) {
            watsonPayload = payload;
        }
    };

    // Function used for initializing the conversation with the first message from Watson
    function initConversation() {
        postConversationMessage('');
    }

    // Send a message request to the server
    function postConversationMessage(text) {
        var data = { 'input': { 'text': text } };
        if (context) {
            data.context = context;
        }
        Api.setUserPayload(data);
        var http = new XMLHttpRequest();
        http.open('POST', messageEndpoint, true);
        http.setRequestHeader('Content-type', 'application/json; charset=utf-8');
        http.onload = function () {
            if (http.status === 200 && http.responseText) {
                var response = JSON.parse(http.responseText);
                context = response.context;
                Api.setWatsonPayload(response);
            } else {
                Api.setWatsonPayload({
                    output: {
                        text: [
                          'The service may be down at the moment; please check' +
                          ' <a href="https://status.ng.bluemix.net/" target="_blank">here</a>' +
                          ' for the current status. <br> If the service is OK,' +
                          ' the app may not be configured correctly,' +
                          ' please check workspace id and credentials for typos. <br>' +
                          ' If the service is running and the app is configured correctly,' +
                          ' try refreshing the page and/or trying a different request.'
                        ]
                    }
                });
                console.error('Server error when trying to reply!');
            }
        };
        http.onerror = function () {
            console.error('Network error trying to send message!');
        };

        http.send(JSON.stringify(data));
    }
}());

/*
 * Copyright © 2016 I.B.M. All rights reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the “License”);
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an “AS IS” BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

/* The Sidebar module handles the display and behavior of the "What can I ask?" sidebar */

/* eslint no-unused-vars: ["error", { "varsIgnorePattern": "^Sidebar$" }] */
/* global Common: true */

var Sidebar = (function () {
    'use strict';

    var ids = {
        sidebar: 'sidebar',
        suggestionList: 'suggestion-list'
    };

    var suggestions = [
      'Turn on the headlights',
      'Shut off my lights',
      'Play some music',
      'What’s my ETA?',
      'Show me what’s nearby',
      'Find a gas station',
      'Turn my radio up'
    ];


    // Publicly accessible methods defined
    return {
        init: init,
        toggle: toggle
    };

    // Initialize the Sidebar module
    function init() {
        populateSuggestions();
    }

    // Populate the suggested user messages in the sidebar
    function populateSuggestions() {
        var suggestionList = document.getElementById(ids.suggestionList);
        for (var i = 0; i < suggestions.length; i++) {
            var listItemJson = {
                'tagName': 'li',
                'children': [{
                    'tagName': 'button',
                    'text': suggestions[i],
                    'classNames': ['suggestion-btn'],
                    'attributes': [{
                        'name': 'onclick',
                        'value': 'Sidebar.toggle(); Conversation.sendMessage("' + suggestions[i] + '")'
                    }]
                }]
            };
            suggestionList.appendChild(Common.buildDomElement(listItemJson));
        }
    }

    // Toggle whether the sidebar is displayed
    function toggle() {
        Common.toggleClass(document.getElementById(ids.sidebar), 'is-active');
    }
}());

/*
 * Copyright © 2016 I.B.M. All rights reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the “License”);
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an “AS IS” BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

/* The TooltipDialogs module handles the display and behavior of the dialog boxes
 * that are used to introduce new users to the system.
 */

/* eslint no-unused-vars: ["error", { "varsIgnorePattern": "^TooltipDialogs$" }] */
/* global Common: true, Conversation: true, Api: true */

var TooltipDialogs = (function () {
    'use strict';

    var ids = {
        tooltipDialogList: 'tooltip-dialog-list',
        darkOverlay: 'dark-overlay',
        clearOverlay: 'clear-overlay'
    };

    var classes = {
        betweenOverlays: 'between-overlays',
        hide: 'hide'
    };

    // Simplified data structure that is used for dialog box population
    var dialogBoxes = [{
        // id attribute for the dialog box
        dialogId: 'welcome-tooltip-dialog',
        // text of the dialog box
        text: 'Hi! I’m Watson. This is a sample application to see how I work. \n\n'
          + ' For this app, imagine you’re driving and I’m your co-pilot, here to help however I can.',
        // id of an element to display while this dialog box is active
        showId: null
    }, {
        dialogId: 'type-here-tooltip-dialog',
        text: 'You can ask questions here.',
        showId: 'input-wrapper'
    }, {
        dialogId: 'menu-here-tooltip-dialog',
        text: 'And if you don’t know what to ask, click here to see what I am trained to understand.',
        showId: 'help'
    }];

    // Object to keep track of which dialog box should be displayed
    var dialogIndex = (function () {
        var index = 0;
        return {
            get: function () {
                return index;
            },
            set: function (integer) {
                index = integer;
                adjustDialogBoxDisplay();
            },
            increment: function () {
                index++;
            }
        };
    })();

    // Publicly accessible methods defined
    return {
        init: init,
        close: closeDialogs,
        next: nextDialog
    };

    // Initilialize the TooltipDialogs module
    function init() {
        populateDialogList();
    }

    // Populate the tooltip dialog boxes
    function populateDialogList() {
        var dialogList = document.getElementById(ids.tooltipDialogList);
        for (var i = 0; i < dialogBoxes.length; i++) {
            var dialogBox = dialogBoxes[i];
            var listItemJson = {
                'tagName': 'div',
                'attributes': [{
                    'name': 'id',
                    'value': dialogBox.dialogId
                }],
                'classNames': (i !== dialogIndex.get()
                  ? ['tooltip-dialog-box', classes.hide]
                  : ['tooltip-dialog-box']),
                'children': [{
                    'tagName': 'img',
                    'text': 'close',
                    'classNames': ['close', 'tooltip-dialog-close'],
                    'attributes': [{
                        'name': 'onclick',
                        'value': 'TooltipDialogs.close()'
                    }, {
                        'name': 'src',
                        'value': 'images/close-button.png'
                    }]
                }, {
                    'tagName': 'div',
                    'classNames': ['tooltip-dialog-text'],
                    'children': [{
                        'tagName': 'p',
                        'classNames': ['pre-bar'],
                        'text': dialogBox.text
                    }]
                }, {
                    'tagName': 'div',
                    'classNames': ['tooltip-dialog-btn-wrapper'],
                    'children': [{
                        'tagName': 'button',
                        'classNames': ['tooltip-dialog-btn'],
                        'attributes': [{
                            'name': 'onclick',
                            'value': 'TooltipDialogs.next()'
                        }],
                        'text': ((i <= dialogBoxes.length) ? 'Next' : 'Done')
                    }]
                }]
            };
            dialogList.appendChild(Common.buildDomElement(listItemJson));
        }
    }

    // Move to the next dialog box in the sequence
    function nextDialog() {
        var oldShow = document.getElementById(dialogBoxes[dialogIndex.get()].showId);
        if (oldShow) {
            Common.removeClass(oldShow, classes.betweenOverlays);
        }
        dialogIndex.increment();
        adjustDialogBoxDisplay();
        if (dialogIndex.get() >= dialogBoxes.length) {
            closeDialogs();
        } else {
            var newShow = document.getElementById(dialogBoxes[dialogIndex.get()].showId);
            if (newShow) {
                Common.addClass(newShow, classes.betweenOverlays);
            }
        }
    }

    // Close out of the dialog box sequence
    function closeDialogs() {
        dialogIndex.set(-1);
        for (var i = 0; i < dialogBoxes.length; i++) {
            var toReset = document.getElementById(dialogBoxes[i].showId);
            if (toReset) {
                Common.removeClass(toReset, classes.betweenOverlays);
            }
        }
        Api.initConversation(); // Load initial Watson greeting after overlays are gone.
        hideOverlays();
        Conversation.focusInput();
    }

    // Adjust the dialog box that is currently displayed (and hide the others)
    function adjustDialogBoxDisplay() {
        for (var i = 0; i < dialogBoxes.length; i++) {
            var currentDialog = document.getElementById(dialogBoxes[i].dialogId);
            if (i === dialogIndex.get()) {
                Common.removeClass(currentDialog, classes.hide);
            } else {
                Common.addClass(currentDialog, classes.hide);
            }
        }
    }

    // Hide the dark semi-transparent overlays
    function hideOverlays() {
        var darkOverlay = document.getElementById(ids.darkOverlay);
        var clearOverlay = document.getElementById(ids.clearOverlay);
        Common.addClass(darkOverlay, classes.hide);
        Common.addClass(clearOverlay, classes.hide);
    }
}());

/*
 * Copyright © 2016 I.B.M. All rights reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the “License”);
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an “AS IS” BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

/* The Animations module handles all animated parts of the app (in the SVG) */

/* eslint no-unused-vars: ["error", { "varsIgnorePattern": "^Animations$" }] */
/* global Common: true, Snap: true, mina: true, Panel: true */

var Animations = (function () {
    'use strict';

    var snapSvgCanvas;
    var state;
    var initialized = false;
    var wiperSpeed;

    // Redraw after every 5 frames.
    var frameSkipRate = 3;

    var classes = {
        drop: 'drop',

        rain: 'rain',
        reduceRain: 'reduce_rain',

        darkGrayCloud: 'darkGrayCloud',
        lightGrayCloud: 'lightGrayCloud',
        lightCloud: 'lightCloud',
        darkCloud: 'darkCloud',

        darkSky: 'darkSky'
    };
    var ids = {
        dottedLine: 'dotted_line',
        stripes1: 'stripes1',
        stripes2: 'stripes2',
        sun: 'sun',
        skyHue: 'skyHue',
        upperDrops: 'upperDrops',
        lowerDrops: 'lowerDrops'
    };
    var idSelectors = {
        svgCanvas: '#svg_canvas',
        speedometer: '#speedometer',
        revmeter: '#revmeter',
        rightNeedle: '#right_needle',
        leftNeedle: '#left_needle',
        tree: '#tree',
        rightWiper: '#right_wiper',
        leftWiper: '#left_wiper',
        headlights: '#headlights',
        upperDrops: '#upperDrops',
        lowerDrops: '#lowerDrops',

        cloud1: '#cloud1',
        cloud2: '#cloud2',
        cloud3: '#cloud3',
        cloud4: '#cloud4',
        cloud5: '#cloud5',
        cloud6: '#cloud6'
    };
    var svgUrls = {
        background: './images/background.svg',
        dashboard: './images/dashboard.svg',
        sky: './images/sky.svg'
    };

    // Publicly accessible methods defined
    return {
        init: init,
        isInitialized: isInitialized,
        toggleRain: toggleRain,
        lightsOn: lightsOn,
        lightsOff: lightsOff,
        wipersOn: wipersOn,
        wipersOff: wipersOff,
        animate_trees: animateTrees,
        initiate_raining: initiateRaining,
        animate_needles: animateNeedles,
        animate_road: animateRoad
    };

    // Initialize the animations
    function init() {
        state = {
            svg_width: 1154,
            svg_height: 335,
            wiping: false,
            num_drops: 100,
            raining: false
        };

        // eslint-disable-next-line new-cap
        snapSvgCanvas = Snap(idSelectors.svgCanvas);

        // Loads sky, then background, then dashboard, using callbacks
        loadSky();
    }


    // Returns true if the Animations module is fully initialized (including full SVG loading)
    function isInitialized() {
        return initialized;
    }

    // Load the dashboard and wipers
    function loadDashboard() {
        // Create SVG group to hold the SVG loaded from file
        var dash = snapSvgCanvas.group();
        Snap.load(svgUrls.dashboard, function (svgFragment) {
            svgFragment.select('title').remove();   // Remove the tooltip from the SVG
            // Append the loaded fragment from file to the SVG group
            dash.append(svgFragment);

            animateNeedles();
            var rightWiper = Snap.select(idSelectors.rightWiper);
            var leftWiper = Snap.select(idSelectors.leftWiper);

            // Remember the initial positioning of wipers
            rightWiper.bbox = rightWiper.getBBox();
            leftWiper.bbox = leftWiper.getBBox();

            state.wipers = {
                right: rightWiper,
                left: leftWiper
            };

            // Draw the Watson log on the panel and set up for animations
            Panel.init();

            initialized = true;
        });
    }

    // Load the background and set the rain to start in 1 minute, lasting for 30 seconds
    function loadBackground() {
        // Create SVG group to hold the SVG loaded from file
        var background = snapSvgCanvas.group();
        Snap.load(svgUrls.background, function (svgFragment) {
            svgFragment.select('title').remove();   // Remove the tooltip from the SVG
            // Append the loaded fragment from file to the SVG group
            background.append(svgFragment);

            // Begin animating the elements
            animateRoad();
            animateTrees();
            animateClouds();

            // Create the rain drops without displaying them
            initiateRaining();

            // Setup a loop to call toggle rain every 30s
            (function rainLoop() {
                setTimeout(function () {
                    toggleRain();
                    setTimeout(function () {
                        toggleRain();
                        rainLoop();
                    }, 60000);
                }, 30000);
            })();

            // Begin loading the dashboard SVGs
            loadDashboard();
        });
    }

    // Loads the sky
    function loadSky() {
        // Create SVG group to hold the SVG loaded from file
        var sky = snapSvgCanvas.group();
        Snap.load(svgUrls.sky, function (svgFragment) {
            svgFragment.select('title').remove();   // Remove the tooltip from the SVG

            // Append the loaded fragment from file to the SVG group
            sky.append(svgFragment);

            // Load the background
            loadBackground();
        });
    }

    // Animates the movement of the road
    function animateRoad() {
        Common.hide(document.getElementById(ids.dottedLine));
        Common.hide(document.getElementById(ids.stripes1));

        // Ever 120ms alternate the positioning of the dotted line
        // To create illusion of a moving road by alternating visibility
        // of sections
        setInterval(function () {
            Common.toggle(document.getElementById(ids.stripes1));
            Common.toggle(document.getElementById(ids.stripes2));
        }, 120);
    }

    // Repeatedly animate movement of cloud by dx over a specified duration
    function moveCloud(cloud, duration, dx) {
        // move cloud to starting position
        cloud.attr({ opacity: 0, transform: 't' + [0, 0] });

        // In 1 tenth of the duration bring opacity to 1 then in the rest move the cloud
        cloud.animate({ opacity: 1 }, 0.1 * duration, mina.linear, function () {
            cloud.animate({ opacity: 0.5, transform: 't' + [dx, 0] }, 0.9 * duration, mina.linear,
              function () {
                  // Repeat the animation from the top
                  cloud.stop();
                  moveCloud(cloud, duration, dx);
              }, frameSkipRate);
        }, frameSkipRate);
    }

    // Start the clouds animations
    function animateClouds() {
        moveCloud(Snap.select(idSelectors.cloud1), 50000, -4500);
        moveCloud(Snap.select(idSelectors.cloud2), 90000, -4500);
        moveCloud(Snap.select(idSelectors.cloud3), 23000, 2000);
        moveCloud(Snap.select(idSelectors.cloud4), 90000, -4500);
        moveCloud(Snap.select(idSelectors.cloud5), 21000, 2000);
        moveCloud(Snap.select(idSelectors.cloud6), 20000, 2000);
    }

    // Repeatedly animates the trees to pass by on the right and left
    function animateTrees() {
        var t = Snap.select(idSelectors.tree);

        // Move to original position and make tree visible
        t.transform('t0,0');
        t.stop();
        t.attr({ display: '' });

        // Randomly chose to move tree on left or right side of the road
        var leftXtransform = [-130, 10];
        var rightXtransform = [120, 10];
        var translate = (Math.random() > 0.5 ? leftXtransform : rightXtransform);

        // Start transforming the trees slowly then faster as the car gets closer
        var easeInExpo = function (n) {
            return (n === 0) ? 0 : Math.pow(2, 10 * (n - 1));
        };

        // Final transform should be scaled 20x and translated
        var endScene = 's20,20,' + 't' + translate;

        // Animate tree to the end scene in 4.5s
        t.animate({ transform: endScene }, 4500, easeInExpo, function () {
            // Hide tree once the animation is complete
            t.attr({ display: 'none' });
            t.stop();

            // Repeat animation
            animateTrees();
        }, frameSkipRate);
    }

    // Create rain objects and then hide the objects (waiting for rain to be toggled on)
    function initiateRaining() {
        makeRain();
        Common.hide(document.getElementById(ids.lowerDrops));
        Common.hide(document.getElementById(ids.upperDrops));
    }

    // Create the raindrop objects
    function makeRain() {
        // Create 2 groups of rain drops. One for the top half and one for the bottom.
        // Each animated slight differently to create illusion of continuity
        var upperDrops = snapSvgCanvas.group();
        var lowerDrops = snapSvgCanvas.group();
        addDropsToGroup(state.num_drops / 2, upperDrops);
        addDropsToGroup(state.num_drops / 2, lowerDrops);

        // Set the IDs for the groups so we can easily identify them in other functions
        upperDrops.node.id = ids.upperDrops;
        lowerDrops.node.id = ids.lowerDrops;
    }

    // Draw count randomly positioned drops and add them to the SVG group
    function addDropsToGroup(count, group) {
        for (var i = 0; i < count; i++) {
            var x = Math.random() * state.svg_width;
            var y = Math.random() * state.svg_height;
            group.append(newDropline(x, y));
        }
    }

    // Function to help in the creation of raindrop objects
    function newDropline(x, y) {
        // randomize sizes of drops
        var scale = 0.1 + 0.3 * Math.random();

        // create the svg path string for drawing the drops
        var dropPath =
          'm,' + [x, y] +
          ',l,' + [0, 0] +
          ' ,c,' + [-3.4105934 * scale, -3.41062 * scale, -3.013645 * scale,
            -9.00921 * scale, 3.810723 * scale, -14.7348 * scale] +
          ',l,' + [68.031 * scale, -57.107 * scale] +
          ',l,' + [-57.107 * scale, 68.034 * scale] +
          ',c,' + [-5.725604 * scale, 6.8212 * scale, -11.324178 * scale,
            7.22133 * scale, -14.734769 * scale, 3.80759 * scale] +
          ',z';

        // Make sure the path dims are relative
        var rel = Snap.path.toRelative(dropPath);
        var drop = snapSvgCanvas.path(rel);

        drop.addClass(classes.drop);
        drop.attr({
            fill: '#ceeaf4'   // give drops the blue color
        });
        return drop;
    }

    // Make the rain start or stop
    function toggleRain() {
        // darken the sky
        toggleDarkenSky();

        var topTransform = [-20, -300];
        var fallDistance = 650;
        var upperDrops = Snap.select(idSelectors.upperDrops);
        var lowerDrops = Snap.select(idSelectors.lowerDrops);

        // Move drops to top of the screen
        upperDrops.transform('t' + topTransform);
        lowerDrops.transform('t' + topTransform);

        // Move the group of upper drops downwards
        function animateUpper() {
            // Reset to top of screen
            upperDrops.transform('t' + topTransform);
            upperDrops.stop();
            Common.show(upperDrops.node);

            // Animate falling movement to bottom of screen
            upperDrops.animate({
                transform: 't' + [Math.random() * 50,
                  topTransform[1] + fallDistance]
            }, 5000, mina.linear, function () {
                Common.hide(upperDrops.node);
            }, frameSkipRate);
        }

        // Begin moving the lower drops downwards then move the upper drops
        function animateDrops() {
            // Reset to top of screen
            lowerDrops.transform('t' + topTransform);
            lowerDrops.stop();
            Common.show(lowerDrops.node);

            // Animate falling of lower drops
            lowerDrops.animate({
                transform: 't' + [Math.random() * 50,
                  topTransform[1] + fallDistance / 2.0]
            }, 2500, mina.linear, function () {
                // begin animation of upper drops half way through the animation
                if (state.raining) {
                    animateUpper();
                }
                lowerDrops.animate({
                    transform: 't' + [Math.random() * 50,
                      topTransform[1] + fallDistance]
                }, 2500, mina.linear, function () {
                    if (state.raining) {
                        animateDrops();
                    } else {
                        Common.hide(lowerDrops.node);
                    }
                }, frameSkipRate);
            }, frameSkipRate);
        }

        if (!state.raining) {
            // start animating the drops
            animateDrops();
        } else {
            // stop the raining
            upperDrops.stop();
            Common.hide(upperDrops.node);
            lowerDrops.stop();
            Common.hide(lowerDrops.node);
        }
        state.raining = !state.raining;
    }

    // Darken the sky to correspond with the rain
    function toggleDarkenSky() {
        // hide the sun and make the clouds darker
        Common.listForEach(document.getElementsByClassName(classes.darkCloud),
          function (currentElement) {
              Common.toggleClass(currentElement, classes.lightGrayCloud);
          });
        Common.listForEach(document.getElementsByClassName(classes.lightCloud),
          function (currentElement) {
              Common.toggleClass(currentElement, classes.darkGrayCloud);
          });
        Common.fadeToggle(document.getElementById(ids.sun));
        Common.toggleClass(document.getElementById(ids.skyHue), classes.darkSky);
    }

    // Set up animations for the speedometer and tachometer
    function animateNeedles() {
        var speedometer = Snap.select(idSelectors.speedometer);
        var revmeter = Snap.select(idSelectors.revmeter);
        var rightNeedle = Snap.select(idSelectors.rightNeedle);
        var leftNeedle = Snap.select(idSelectors.leftNeedle);

        // Stop any running animations
        rightNeedle.stop();
        leftNeedle.stop();

        // Animate the needles around the center of the dials in a range
        // of 10-110 randomly
        leftNeedle.animate({
            transform: 'r' + ((30 * Math.random()) - 30) + ','
            + revmeter.getBBox().cx + ',' + revmeter.getBBox().cy
        }, 9000, mina.easeinout, function () { }, frameSkipRate);
        rightNeedle.animate({
            transform: 'r' + ((45 * Math.random()) - 30) + ', '
            + speedometer.getBBox().cx + ',' + speedometer.getBBox().cy
        },
          9000 * Math.random(), mina.easeinout, function () {
              // Repeat the animation
              animateNeedles();
          }, frameSkipRate);
    }

    // Turn headlights on
    function lightsOn() {
        // Set the light to visible and fade in over 300ms
        Snap.select(idSelectors.headlights).attr({ display: '', opacity: 0 });
        Snap.select(idSelectors.headlights).animate({ opacity: 1 }, 300, mina.linear);
    }

    // Turn headlights off
    function lightsOff() {
        // Fade out the light over 500s
        Snap.select(idSelectors.headlights).animate({ opacity: 0 }, 500, mina.linear, function () {
            // After fading, hide the light from the DOM
            Snap.select(idSelectors.headlights).attr({ display: 'none' });
        });
    }

    // Turn wipers on
    function wipersOn(speed) {
        if (!state.wiping) {
            state.wiping = true;  // Signal to enter the wiping state
        }
        setWiperSpeed(speed);

        if (state.wipingAnim) { // If animation is already going on return
            return;
        }
        moveWipers();
    }

    function setWiperSpeed(speed) {
        wiperSpeed = speed;
    }

    // Turn wipers off
    function wipersOff() {
        if (!state.wiping) {
            return;
        }
        state.wiping = false;
    }

    // Rotate the wipers in degrees from -> to, then execute the next callback
    function rotateWipers(from, to, next) {
        var rWiper = state.wipers.right;
        var lWiper = state.wipers.left;
        var speeds = {
            hi: 2,
            lo: 1
        };

        // Stop any running animation first
        if (state.wipingAnim) {
            state.wipingAnim.stop();
        }

        // Begin the wiping animation
        state.wipingAnim = Snap.animate(from, to, function (val) {
            rWiper.transform('r' + [val, rWiper.bbox.x + rWiper.bbox.w, rWiper.bbox.y + rWiper.bbox.h]);
            lWiper.transform('r' + [val, lWiper.bbox.x + lWiper.bbox.w, lWiper.bbox.y + lWiper.bbox.h]);
        }, 2000 / Math.max(speeds[wiperSpeed], speeds.lo), mina.linear, next, frameSkipRate);
    }

    // Repeatedly animates movement of the wipers back and fourth
    function moveWipers() {
        // check if the user has called wipers off
        if (!state.wiping) {
            state.wipingAnim = false; // signal that the animations is over
            return;
        }
        // rotate the wipers 170 degrees back then restart moveWipers
        function back() {
            rotateWipers(170, 0, moveWipers);
        }
        // rotate  the wipers 170 degrees forward then call the back function
        function forward() {
            rotateWipers(0, 170, back);
        }

        // Kick off the animation with a forward animation
        forward();
    }
}());

/*
 * Copyright © 2016 I.B.M. All rights reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the “License”);
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an “AS IS” BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

/* The Panel module involves the display and behavior of the dashboard panel within the SVG */

/* eslint no-unused-vars: ["error", { "varsIgnorePattern": "^Panel$" }] */
/* global mina: true, Snap: true, Common: true */

var Panel = (function () {
    var ids = {
        defaultScreen: 'defaultScreen',
        panelGenreText: 'panel-genre-text'
    };
    var idSelectors = {
        svgCanvas: '#svg_canvas',
        panel: '#panel',
        fan: '#fan',
        seek: '#seek'
    };
    var genres = ['general', 'classical', 'jazz', 'rock', 'pop'];
    var snapSvgCanvas = Snap.select(idSelectors.svgCanvas);

    var frameSkipRate = 5;  // redraw after 5 frames
    var currentAnimations = [];

    // Publicly accessible methods defined
    return {
        playMusic: playMusic,
        ac: ac,
        heat: heat,
        mapFoodNumbers: mapFoodNumbers,
        mapFoodCuisine: mapFoodCuisine,
        mapGas: mapGas,
        mapRestrooms: mapRestrooms,
        mapGeneral: mapGeneral,
        mapNavigation: mapNavigation,
        init: defaultScreen,
        setWatsonPanelToDefault: setWatsonPanelToDefault
    };

    // clear everything on the panel until only the Watson logo is left
    function clearToDefault(panel) {
        // Stop all animations first
        Common.listForEach(currentAnimations, function (element) {
            element.stop();
        });
        currentAnimations = [];

        // Clear to default Watson logo
        Common.listForEach(panel.node.childNodes, function (element) {
            if (element.id !== ids.defaultScreen) {
                panel.node.removeChild(element);
            }
        });
    }

    function setWatsonPanelToDefault() {
        var p = Snap.select(idSelectors.panel);
        clearToDefault(p);
    }

    // Auxiliary function for loading an SVG into the panel
    function loadSvg(filename, next) {
        // Clear the panel console display and leave on the Watson logo
        var p = Snap.select(idSelectors.panel);
        clearToDefault(p);

        // Create a new SVG group to hold the loaded SVG
        var svgGroup = snapSvgCanvas.group();

        Snap.load('./images/' + filename + '.svg', function (svgFragment) {
            svgFragment.select('title').remove();   // Remove the tooltip from the svg

            // Position the SVG group on the panel console
            svgGroup.append(svgFragment);
            svgGroup.transform('T' + [180, 137] + 's0.29,0.29');
            p.append(svgGroup);

            // Place a rectangular mask around the panel console area to clip off any bits
            // of the SVG group That are not within the panel console area
            var panelMask = svgGroup.rect(60, 15, 910, 680, 20, 20).attr({ 'strokeWidth': 0, fill: 'white' });
            svgGroup.attr({ mask: panelMask });

            // Fade in the SVG group
            svgGroup.attr({ opacity: 0 });
            var fadeAnimation = svgGroup.animate({ opacity: 1 }, 700, mina.linear, function () { }, frameSkipRate);
            currentAnimations.push(fadeAnimation);
            // Execute callback if provided
            if (next) {
                next(svgFragment, svgGroup);
            }
        });
        return svgGroup;
    }

    // Rotate the fan in the svgGroup at the speed specified by level
    function animateFan(level, svgGroup) {
        // Find the fan in the DOM and get its initial coordinates
        var fan = Snap.select(idSelectors.fan);
        var bbox = fan.getBBox();

        // TODO Speeds seem to be much faster in Chrome than FF
        var speed = {
            hi: 20,
            lo: 10
        }[level];

        var doneFade = false;
        var rotateAnim = Snap.animate(0, 100, function (val) {
            // At 90% of the animation apply the fade animation once to
            // Begin fading out the SVG group from the panel display
            if (val > 90) {
                if (!doneFade) {
                    var fadeAnim = svgGroup.animate({ opacity: 0 }, 500, mina.linear, function () {
                        svgGroup.remove();
                    }, frameSkipRate);
                    doneFade = true;
                    currentAnimations.push(fadeAnim);
                }
            }
            // Rotate the fan around its center (bbox.cx, bbox.cy) at the speed given
            var localMat = fan.transform().localMatrix;
            fan.transform(localMat.rotate(speed, bbox.cx, bbox.cy));
        }, 30000, mina.linear, function () { }, frameSkipRate);
        currentAnimations.push(rotateAnim);
    }

    // Show that music of the given genre is playing
    function playMusic(genre) {
        // Define a callback for the loading function
        function next(svgFragment, svgGroup) {
            var genreText = document.getElementById(ids.panelGenreText);
            if (genreText) {
                genreText.textContent = genre.toUpperCase();
            }

            var seek = Snap.select(idSelectors.seek);
            var localMat = seek.transform().localMatrix;

            // Animate moving the seek position
            var seekAnimation = seek.animate({ transform: localMat.translate(1050, 0) }, 30000, mina.linear, function () {
                // After the seek position has reached the end fade out the SVG group
                var fadeAnimation = svgGroup.animate({ opacity: 0 }, 500, mina.linear, function () {
                    svgGroup.remove();
                }, frameSkipRate);
                currentAnimations.push(fadeAnimation);
            }, frameSkipRate);
            currentAnimations.push(seekAnimation);
        }

        var genreStr = genre;
        if (genres.indexOf(genreStr) < 0) {
            genreStr = 'genre';
        }

        // Load the SVG then execute the next callback
        loadSvg('music ' + genreStr, next);
    }

    // Turn on A/C
    function ac(level) {
        loadSvg('ac ' + level, function (svgFragment, svgGroup) {
            animateFan(level, svgGroup);
        });
    }

    // Turn on heat
    function heat(level) {
        loadSvg('heat ' + level, function (svgFragment, svgGroup) {
            animateFan(level, svgGroup);
        });
    }

    // Show Watson logo on panel
    function defaultScreen() {
        loadSvg('default screen', function (svgFragment, svgGroup) {
            svgGroup.node.id = ids.defaultScreen;
        });
    }

    // Show the map of food locations numbered
    function mapFoodNumbers() {
        loadSvg('map food numbers');
    }

    // Show the map of food locations by kind
    function mapFoodCuisine() {
        loadSvg('map food cuisine');
    }

    // Show the map of gas stations
    function mapGas() {
        loadSvg('map gas');
    }

    // Show the map of restrooms
    function mapRestrooms() {
        loadSvg('map restrooms');
    }

    // Show the map of the surrounding area
    function mapGeneral() {
        loadSvg('map general');
    }

    // Set a given choice (e.g first, second e.t.c) as the current goal on the
    // Map
    function mapNavigation(choice) {
        Snap.selectAll('.nav_active').forEach(function (e) {
            e.removeClass('nav_active');
        });

        var goal = Snap.select('#' + choice);

        if (goal) {
            goal.addClass('nav_active');
        }
    }
})();

/*
 * Copyright © 2016 I.B.M. All rights reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the “License”);
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an “AS IS” BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

/* The Conversation module handles the display and behavior of the chat section
 * of the application, including the messages to and from Watson and the input box
 */

/* eslint no-unused-vars: ["error", { "varsIgnorePattern": "^Conversation$" }] */
/* global Api: true, Common: true */


var Conversation = (function () {
    'use strict';
    var ids = {
        userInput: 'user-input',
        userInputDummy: 'user-input-dummy',
        chatFlow: 'chat-flow',
        chatScrollWrapper: 'chat-scroll-wrapper'
    };
    var classes = {
        messageWrapper: 'message-wrapper',
        preBar: 'pre-bar',
        underline: 'underline'
    };
    var authorTypes = {
        user: 'user',
        watson: 'watson'
    };

    // Publicly accessible methods defined
    return {
        init: init,
        setMessage: setMessage,
        sendMessage: sendMessage,
        focusInput: focusInput
    };

    // Initialize Conversation module
    function init() {
        chatSetup();
        initEnterSubmit();
        setupInputBox();
    }

    // Hide chat box until there are messages,
    // set up messages to display when user or Watson sends message
    function chatSetup() {
        document.getElementById(ids.chatScrollWrapper).style.display = 'none';

        var currentRequestPayloadSetter = Api.setUserPayload;
        Api.setUserPayload = function (payload) {
            currentRequestPayloadSetter.call(Api, payload);
            displayMessage(payload, authorTypes.user);
        };

        var currentResponsePayloadSetter = Api.setWatsonPayload;
        Api.setWatsonPayload = function (payload) {
            currentResponsePayloadSetter.call(Api, payload);
            displayMessage(payload, authorTypes.watson);
        };
    }

    // Set up the input box to submit a message when enter is pressed
    function initEnterSubmit() {
        document.getElementById(ids.userInput)
            .addEventListener('keypress', function (event) {
                if (event.keyCode === 13) {
                    sendMessage();
                    event.preventDefault();
                }
            }, false);
    }

    // Set up the input box to underline text as it is typed
    // This is done by creating a hidden dummy version of the input box that
    // is used to determine what the width of the input text should be.
    // This value is then used to set the new width of the visible input box.
    function setupInputBox() {
        var input = document.getElementById(ids.userInput);
        var dummy = document.getElementById(ids.userInputDummy);
        var minFontSize = 9;
        var maxFontSize = 16;
        var minPadding = 5;
        var maxPadding = 9;

        // If no dummy input box exists, create one
        if (dummy === null) {
            var dummyJson = {
                'tagName': 'div',
                'attributes': [{
                    'name': 'id',
                    'value': (ids.userInputDummy)
                }]
            };

            dummy = Common.buildDomElement(dummyJson);
            document.body.appendChild(dummy);
        }

        function adjustInput() {
            if (input.value === '') {
                // If the input box is empty, remove the underline
                Common.removeClass(input, 'underline');
                input.setAttribute('style', 'width:' + '100%');
                input.style.width = '100%';
            } else {
                // otherwise, adjust the dummy text to match, and then set the width of
                // the visible input box to match it (thus extending the underline)
                Common.addClass(input, classes.underline);
                var txtNode = document.createTextNode(input.value);
                ['font-size', 'font-style', 'font-weight', 'font-family', 'line-height',
                  'text-transform', 'letter-spacing'].forEach(function (index) {
                      dummy.style[index] = window.getComputedStyle(input, null).getPropertyValue(index);
                  });
                dummy.textContent = txtNode.textContent;

                var padding = 0;
                var htmlElem = document.getElementsByTagName('html')[0];
                var currentFontSize = parseInt(window.getComputedStyle(htmlElem, null).getPropertyValue('font-size'), 10);
                if (currentFontSize) {
                    padding = Math.floor((currentFontSize - minFontSize) / (maxFontSize - minFontSize)
                      * (maxPadding - minPadding) + minPadding);
                } else {
                    padding = maxPadding;
                }

                var widthValue = (dummy.offsetWidth + padding) + 'px';
                input.setAttribute('style', 'width:' + widthValue);
                input.style.width = widthValue;
            }
        }

        // Any time the input changes, or the window resizes, adjust the size of the input box
        input.addEventListener('input', adjustInput);
        window.addEventListener('resize', adjustInput);

        // Trigger the input event once to set up the input box and dummy element
        Common.fireEvent(input, 'input');
    }

    // Retrieve the value of the input box
    function getMessage() {
        var userInput = document.getElementById(ids.userInput);
        return userInput.value;
    }

    // Set the value of the input box
    function setMessage(text) {
        var userInput = document.getElementById(ids.userInput);
        userInput.value = text;
        userInput.focus();
        Common.fireEvent(userInput, 'input');
    }

    // Send the message from the input box
    function sendMessage(newText) {
        var text;
        if (newText) {
            text = newText;
        } else {
            text = getMessage();
        }
        if (!text) {
            return;
        }
        setMessage('');

        Api.postConversationMessage(text);
    }

    // Display a message, given a message payload and a message type (user or Watson)
    function displayMessage(newPayload, typeValue) {
        var isUser = isUserMessage(typeValue);
        var textExists = (newPayload.input && newPayload.input.text)
          || (newPayload.output && newPayload.output.text);
        if (isUser !== null && textExists) {
            if (newPayload.output && Object.prototype.toString.call(newPayload.output.text) === '[object Array]') {
                newPayload.output.text = newPayload.output.text.filter(function (item) {
                    return item && item.length > 0;
                }).join(' ');
            }
            var dataObj = isUser ? newPayload.input : newPayload.output;

            if (!String(dataObj.text).trim()) {
                return;
            }
            var messageDiv = buildMessageDomElement(newPayload, isUser);


            var chatBoxElement = document.getElementById(ids.chatFlow);
            chatBoxElement.appendChild(messageDiv);
            updateChat();
        }
    }

    // Determine whether a given message type is user or Watson
    function isUserMessage(typeValue) {
        if (typeValue === authorTypes.user) {
            return true;
        } else if (typeValue === authorTypes.watson) {
            return false;
        }
        return null;
    }

    // Builds the message DOM element (using auxiliary function Common.buildDomElement)
    function buildMessageDomElement(newPayload, isUser) {
        var dataObj = isUser ? newPayload.input : newPayload.output;
        var messageJson = {
            // <div class='user / watson'>
            'tagName': 'div',
            'classNames': ['message-wrapper', (isUser ? authorTypes.user : authorTypes.watson)],
            'children': [{
                // <p class='user-message / watson-message'>
                'tagName': 'p',
                'classNames': (isUser
                  ? [authorTypes.user + '-message']
                  : [authorTypes.watson + '-message', classes.preBar]),
                'html': (isUser ? '<img src=\'/images/head.svg\' />' + dataObj.text : dataObj.text)
            }]
        };

        return Common.buildDomElement(messageJson);
    }

    // Display the chat box if it's currently hidden
    // (i.e. if this is the first message), scroll to the bottom of the chat
    function updateChat() {
        document.getElementById(ids.chatScrollWrapper).style.display = '';
        var messages = document.getElementById(ids.chatFlow).getElementsByClassName(classes.messageWrapper);
        document.getElementById(ids.chatFlow).scrollTop = messages[messages.length - 1].offsetTop;
    }

    // Set browser focus on the input box
    function focusInput() {
        document.getElementById(ids.userInput).focus();
    }
}());

/*
 * Copyright © 2016 I.B.M. All rights reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the “License”);
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an “AS IS” BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

/* The Intents module contains a list of the possible intents that might be returned by the API */

/* eslint no-unused-vars: ["error", { "varsIgnorePattern": "^ConversationResponse$" }] */
/* global Animations: true, Api: true, Panel: true */

var ConversationResponse = (function () {
    'use strict';

    return {
        init: init,
        responseHandler: responseHandler
    };

    function init() {
        setupResponseHandling();
    }

    function actionFunctions(action) {
        if (action.cmd === 'music_on') {
            Panel.playMusic(action.arg);
        } else if (action.cmd === 'wipers_on') {// turn on commands
            Animations.wipersOn('lo');
        } else if (action.cmd === 'lights_on') {
            Animations.lightsOn();
        } else if (action.cmd === 'AC_on') {
            Panel.ac('lo');
        } else if (action.cmd === 'heater_on') {
            Panel.heat('lo');
        } else if (action.cmd === 'fan_on') {
            Panel.ac('lo');
        } else if (action.cmd === 'music_off') {//turn off commands
            Panel.setWatsonPanelToDefault();
        } else if (action.cmd === 'wipers_off') {
            Animations.wipersOff();
        } else if (action.cmd === 'lights_off') {
            Animations.lightsOff();
        } else if (action.cmd === 'AC_off') {
            Panel.setWatsonPanelToDefault();
        } else if (action.cmd === 'heater_off') {
            Panel.setWatsonPanelToDefault();
        } else if (action.cmd === 'fan_off') {
            Panel.setWatsonPanelToDefault();
        } else if (action.cmd === 'music_up') {//turn up commands
            Panel.playMusic('general');
        } else if (action.cmd === 'wipers_up') {
            Animations.wipersOn('hi');
        } else if (action.cmd === 'AC_up') {
            Panel.ac('hi');
        } else if (action.cmd === 'heater_up') {
            Panel.heat('hi');
        } else if (action.cmd === 'fan_up') {
            Panel.ac('hi');
        } else if (action.cmd === 'music_down') {//turn down commands
            Panel.playMusic('general');
        } else if (action.cmd === 'wipers_down') {
            Animations.wipersOn('lo');
        } else if (action.cmd === 'AC_down') {
            Panel.ac('lo');
        } else if (action.cmd === 'heater_down') {
            Panel.heat('lo');
        } else if (action.cmd === 'fan_down') {
            Panel.ac('lo');
        } else if (action.cmd === 'gas') {// amenity
            Panel.mapGas();
        } else if (action.cmd === 'restaurant') {
            Panel.mapFoodCuisine();
        } else if (action.cmd === 'restroom') {
            Panel.mapRestrooms();
        }
    }

    // Create a callback when a new Watson response is received to handle Watson's response
    function setupResponseHandling() {
        var currentResponsePayloadSetter = Api.setWatsonPayload;
        Api.setWatsonPayload = function (payload) {
            currentResponsePayloadSetter.call(Api, payload);
            responseHandler(payload);
        };
    }



    // Called when a Watson response is received, manages the behavior of the app based
    // on the user intent that was determined by Watson
    function responseHandler(data) {

        let action = data.output.action;


        if (data && !data.output.error) {
            // Check if message is handled by retrieve and rank and there is no message set
            if (action && !data.output.text) {
                // TODO add EIR link
                data.output.text = ['I am not able to answer that. You can try asking the'
                + ' <a href="https://conversation-with-discovery.mybluemix.net/" target="_blank">Information Retrieval with Discovery App</a>'];

                Api.setWatsonPayload(data);
                return;
            }



            if (action) {
                let actionArray = getActions(action);
                if (actionArray) {
                    for (let i in actionArray) {
                        if (actionArray.hasOwnProperty(i)) {
                            actionFunctions(actionArray[i]);
                        }
                    }
                }
            }
        }
    }

    function getActions(action) {
        let res = {};

        let cnt = 0;

        for (let key in action) {
            if (action.hasOwnProperty(key)) {
                res[cnt] = {
                    cmd: key,
                    arg: action[key]
                };
                cnt++;
            }
        }
        return res;
    }
}());
/*
 * Copyright © 2016 I.B.M. All rights reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the “License”);
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an “AS IS” BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

/* The Global module is used to initialize the other modules */

/* global TooltipDialogs: true, Conversation: true, ConversationResponse: true, Sidebar: true, Animations: true, Common: true */

(function () {
    TooltipDialogs.init();
    Conversation.init();
    ConversationResponse.init();
    Sidebar.init();
    Animations.init();
    // Used as a cloak to delay displaying the app until it's likely done rendering
    Common.wait(Animations.isInitialized, function () {
        document.body.style.visibility = 'visible';
    }, 50);
}());

/*
 * Copyright © 2016 I.B.M. All rights reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the “License”);
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an “AS IS” BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
/* global Conversation: true, Animations: true, Panel: true*/
/* eslint no-unused-vars: ["error", { "varsIgnorePattern": "^tests$" }] */

var tests = (function () {
    var time = 0;

    return {
        run: runTests,
        testPanel: testPanel,
        testDash: testDash,
        testConversation: testConversation
    };

    function runTests() {
        testPanel();
        testDash();
        testConversation();
    }

    function doTest(func, duration, label) {
        var durationMs = duration * 1000;

        setTimeout(function () {
            // TODO remove console logging
            console.log('test: ' + label);
            func();
        }, time);
        time += durationMs + 1000;
    }

    function testPanel() {
        doTest(function () { Panel.playMusic('jazz'); }, 5, 'play jazz');
        doTest(function () { Panel.playMusic('pop'); }, 5, 'play pop');
        doTest(function () { Panel.playMusic('rock'); }, 5, 'play rock');
        doTest(function () { Panel.playMusic('general'); }, 5, 'play general');
        doTest(function () { Panel.ac('hi'); }, 5, 'ac hi');
        doTest(function () { Panel.ac('lo'); }, 5, 'ac lo');
        doTest(function () { Panel.heat('hi'); }, 5, 'heat hi');
        doTest(function () { Panel.heat('lo'); }, 5, 'heat lo');
        doTest(function () { Panel.mapFoodCuisine(); }, 5, 'map food cuisine');
        doTest(function () { Panel.mapGas(); }, 5, 'map gas');
        doTest(function () { Panel.mapFoodNumbers(); }, 5, 'map food numbers');
    }

    function testDash() {
        // run a series of on/offs on the wipers/lights area
        doTest(function () { Animations.lightsOff(); }, 2, 'light off');
        doTest(function () { Animations.lightsOn(); }, 2, 'light on');
        doTest(function () { Animations.lightsOff(); }, 2, 'light off');
        doTest(function () { Animations.lightsOn(); }, 2, 'light on');
        doTest(function () { Animations.wipersOff(); }, 3, 'wipers off');
        doTest(function () { Animations.wipersOn('lo'); }, 4, 'wipers on lo');
        doTest(function () { Animations.wipersOff(); }, 4, 'wipers off');
        doTest(function () { Animations.wipersOn('hi'); }, 4, 'wipers on hi');
        doTest(function () { Animations.wipersOff(); }, 4, 'wipers off');
        doTest(function () { Animations.toggleRain(); }, 2, 'toggle rain');
        doTest(function () { Animations.wipersOn('lo'); }, 2, 'wipers on lo');
        doTest(function () { Animations.toggleRain(); }, 1, 'toggle rain');
        doTest(function () { Animations.wipersOff(); }, 2, 'wipers off');
        doTest(function () { Animations.lightsOff(); }, 2, 'lights off');
    }

    function testConversation() {
        var suggestions = [
          'Turn on the headlights',
          'Play some music',
          'jazz',
          'Turn on the AC',
          'When will the rain end?',
          'What’s my ETA?',
          'Find a gas station',
          'Make a phone call',
          'Send a text',
          '1',
          'Turn on the hazard lights',
          'Turn on the radio',
          'pop',
          'turn on the wipers',
          'turn off the wipers'
        ];

        var sendSuggestion = function (index) {
            return function () {
                // TODO remove console logging
                console.log('using suggestion: ' + suggestions[index]);
                Conversation.sendMessage(suggestions[index]);
            };
        };

        for (var i = 0; i < suggestions.length; i++) {
            doTest(sendSuggestion(i), 10, suggestions[i]);
        }
    }
})();
