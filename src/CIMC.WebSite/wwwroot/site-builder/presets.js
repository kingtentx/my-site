(function (window) {
    'use strict';
    var root = window.SiteBuilder = window.SiteBuilder || {};
    var R = root.Registry;

    function n(type, props, style, children) {
        var node = R.create(type);
        if (props) Object.keys(props).forEach(function (k) { node.props[k] = props[k]; });
        if (style) Object.keys(style).forEach(function (k) { node.style[k] = style[k]; });
        node.children = children || [];
        return node;
    }

    var presets = {
        heroSplit: {
            name: '左右图文 Hero', group: '常用区块',
            create: function () {
                return n('section', null, { paddingTop:'72px', paddingBottom:'72px', backgroundColor:'#f7f9fc' }, [
                    n('container', null, null, [
                        n('grid', {columns:2}, {gap:'48px'}, [
                            n('column', null, null, [
                                n('heading',{text:'用更灵活的页面构建能力表达品牌价值',level:1},{marginBottom:'20px'}),
                                n('text',{text:'通过区段、容器、网格和基础组件自由组合，不再受固定模板限制。'},{marginBottom:'28px'}),
                                n('button',{text:'了解更多',href:'#',variant:'primary'})
                            ]),
                            n('column', null, null, [n('image',{src:'',alt:'Hero 图片'})])
                        ])
                    ])
                ]);
            }
        },
        featureGrid: {
            name: '三列能力区块', group: '常用区块',
            create: function () {
                return n('section', null, {paddingTop:'64px',paddingBottom:'64px'}, [n('container', null, null, [
                    n('heading',{text:'核心能力',level:2},{textAlign:'center',marginBottom:'36px'}),
                    n('grid',{columns:3},{gap:'24px'}, [
                        n('column',null,{paddingTop:'24px',paddingRight:'24px',paddingBottom:'24px',paddingLeft:'24px'},[n('icon',{text:'◆',size:32}),n('heading',{text:'灵活布局',level:3}),n('text',{text:'使用可嵌套布局组件构建不同页面结构。'})]),
                        n('column',null,{paddingTop:'24px',paddingRight:'24px',paddingBottom:'24px',paddingLeft:'24px'},[n('icon',{text:'◇',size:32}),n('heading',{text:'统一样式',level:3}),n('text',{text:'所有组件共享统一的间距、颜色、尺寸和对齐设置。'})]),
                        n('column',null,{paddingTop:'24px',paddingRight:'24px',paddingBottom:'24px',paddingLeft:'24px'},[n('icon',{text:'○',size:32}),n('heading',{text:'内容绑定',level:3}),n('text',{text:'文章、产品和招聘组件可以独立绑定内容数据。'})])
                    ])
                ])]);
            }
        },
        standardHeader: {
            name: '标准 Header', group: '全局区块',
            create: function () {
                return n('section',null,{paddingTop:'16px',paddingBottom:'16px',backgroundColor:'#ffffff'},[n('container',null,null,[n('grid',{columns:3},{gap:'20px'},[
                    n('column',null,null,[n('logo',{text:'企业名称',href:'/'})]),
                    n('column',null,{textAlign:'center'},[n('navigation',{menuKey:'main'})]),
                    n('column',null,{textAlign:'right'},[n('search',{placeholder:'搜索'}),n('button',{text:'联系我们',href:'/contact',variant:'outline'})])
                ])])]);
            }
        },
        enterpriseFooter: {
            name: '企业 Footer', group: '全局区块',
            create: function () {
                return n('section',null,{paddingTop:'56px',paddingBottom:'28px',backgroundColor:'#111827',color:'#ffffff'},[n('container',null,null,[
                    n('grid',{columns:3},{gap:'40px'},[n('column',null,null,[n('logo',{text:'企业名称',href:'/'}),n('text',{text:'专业、可靠、持续创新。'})]),n('column',null,null,[n('navigation',{menuKey:'footer',direction:'vertical'})]),n('column',null,null,[n('contact',{phone:'',email:'',address:''})])]),
                    n('divider'),n('copyright',{text:'© 2026 企业名称 版权所有'},{textAlign:'center'})
                ])]);
            }
        }
    };

    root.Presets = {
        all: function () { return Object.keys(presets).map(function (key) { return { key:key, name:presets[key].name, group:presets[key].group }; }); },
        create: function (key) { return presets[key] ? presets[key].create() : null; }
    };
})(window);