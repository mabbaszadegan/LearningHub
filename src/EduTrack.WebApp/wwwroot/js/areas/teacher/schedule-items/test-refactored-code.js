/**
 * Test file for refactored content management classes
 * This file tests the integration between shared classes and refactored files
 */

// Test function to verify all classes are available
function testRefactoredClasses() {
    console.log('=== Testing Refactored Classes ===');
    
    // Test if shared classes are available
    const sharedClasses = [
        'FieldManager',
        'EventManager', 
        'PreviewManager',
        'ContentSyncManager'
    ];
    
    sharedClasses.forEach(className => {
        if (typeof window[className] === 'function') {
            console.log(`✅ ${className} is available`);
        } else {
            console.error(`❌ ${className} is not available`);
        }
    });
    
    // Test if main classes are available
    const mainClasses = [
        'ContentBuilderBase',
        'Step4ContentManager'
    ];
    
    mainClasses.forEach(className => {
        if (typeof window[className] === 'function') {
            console.log(`✅ ${className} is available`);
        } else {
            console.error(`❌ ${className} is not available`);
        }
    });
    
    console.log('=== Test Complete ===');
}

// Test function to verify field manager functionality
function testFieldManager() {
    console.log('=== Testing FieldManager ===');
    
    try {
        const fieldManager = new FieldManager();
        
        // Test field registration
        const testField = document.createElement('input');
        testField.id = 'testField';
        testField.value = 'test value';
        document.body.appendChild(testField);
        
        fieldManager.registerField('testField', testField, {
            required: true,
            validate: (value) => {
                if (!value || value.trim() === '') {
                    return { isValid: false, message: 'Field is required' };
                }
                return { isValid: true };
            }
        });
        
        // Test field operations
        const value = fieldManager.getFieldValue('testField');
        console.log('Field value:', value);
        
        const updateResult = fieldManager.updateField('testField', 'new value');
        console.log('Update result:', updateResult);
        
        // Cleanup
        document.body.removeChild(testField);
        fieldManager.clearAllErrors();
        
        console.log('✅ FieldManager test passed');
    } catch (error) {
        console.error('❌ FieldManager test failed:', error);
    }
    
    console.log('=== FieldManager Test Complete ===');
}

// Test function to verify event manager functionality
function testEventManager() {
    console.log('=== Testing EventManager ===');
    
    try {
        const eventManager = new EventManager();
        
        let eventReceived = false;
        
        // Test event listener
        eventManager.addListener('testEvent', (e) => {
            eventReceived = true;
            console.log('Test event received:', e.detail);
        });
        
        // Test event dispatch
        eventManager.dispatch('testEvent', { test: 'data' });
        
        // Wait a bit for event to be processed
        setTimeout(() => {
            if (eventReceived) {
                console.log('✅ EventManager test passed');
            } else {
                console.error('❌ EventManager test failed - event not received');
            }
            
            // Cleanup
            eventManager.removeAllListenersAll();
        }, 100);
        
    } catch (error) {
        console.error('❌ EventManager test failed:', error);
    }
    
    console.log('=== EventManager Test Complete ===');
}

// Test function to verify preview manager functionality
function testPreviewManager() {
    console.log('=== Testing PreviewManager ===');
    
    try {
        const previewManager = new PreviewManager();
        
        // Test preview HTML generation
        const testContent = {
            blocks: [
                {
                    type: 'text',
                    data: {
                        content: 'Test content'
                    }
                }
            ]
        };
        
        const previewHTML = previewManager.generatePreviewHTML(testContent);
        console.log('Preview HTML generated:', previewHTML.length > 0);
        
        console.log('✅ PreviewManager test passed');
    } catch (error) {
        console.error('❌ PreviewManager test failed:', error);
    }
    
    console.log('=== PreviewManager Test Complete ===');
}

// Test function to verify content sync manager functionality
function testContentSyncManager() {
    console.log('=== Testing ContentSyncManager ===');
    
    try {
        const fieldManager = new FieldManager();
        const eventManager = new EventManager();
        const syncManager = new ContentSyncManager(fieldManager, eventManager);
        
        let syncCallbackCalled = false;
        
        // Test sync callback registration
        syncManager.registerSyncCallback('testSync', () => {
            syncCallbackCalled = true;
            console.log('Sync callback called');
        });
        
        // Test sync trigger
        syncManager.sync('test');
        
        // Wait a bit for sync to complete
        setTimeout(() => {
            if (syncCallbackCalled) {
                console.log('✅ ContentSyncManager test passed');
            } else {
                console.error('❌ ContentSyncManager test failed - sync callback not called');
            }
        }, 100);
        
    } catch (error) {
        console.error('❌ ContentSyncManager test failed:', error);
    }
    
    console.log('=== ContentSyncManager Test Complete ===');
}

// Run all tests
function runAllTests() {
    console.log('🚀 Starting Refactored Code Tests...');
    
    testRefactoredClasses();
    
    setTimeout(() => {
        testFieldManager();
    }, 100);
    
    setTimeout(() => {
        testEventManager();
    }, 200);
    
    setTimeout(() => {
        testPreviewManager();
    }, 300);
    
    setTimeout(() => {
        testContentSyncManager();
    }, 400);
    
    setTimeout(() => {
        console.log('🎉 All tests completed!');
    }, 1000);
}

// Export test functions for manual testing
if (typeof window !== 'undefined') {
    window.testRefactoredClasses = testRefactoredClasses;
    window.testFieldManager = testFieldManager;
    window.testEventManager = testEventManager;
    window.testPreviewManager = testPreviewManager;
    window.testContentSyncManager = testContentSyncManager;
    window.runAllTests = runAllTests;
}

// Auto-run tests if this file is loaded directly
if (typeof window !== 'undefined' && window.location.pathname.includes('schedule-items')) {
    setTimeout(runAllTests, 2000);
}
